using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline
{
    public static class UnityGeneratedAssetImporter
    {
        public static void ImportAndCreatePrefab(GenerationJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            if (string.IsNullOrWhiteSpace(job.fbxAssetPath))
                throw new InvalidOperationException("The job has no FBX asset path.");

            EnsureOutputFolders(job);
            AssetDatabase.ImportAsset(job.fbxAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(job.fbxAssetPath) as ModelImporter;
            if (importer == null)
                throw new InvalidOperationException("Unity did not create a ModelImporter for " + job.fbxAssetPath);

            bool animated = job.RequiresMesh2Motion;
            importer.useFileScale = true;
            importer.importAnimation = animated;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.animationType = !animated
                ? ModelImporterAnimationType.None
                : job.kind == GeneratedAssetKind.Human
                    ? ModelImporterAnimationType.Human
                    : ModelImporterAnimationType.Generic;
            if (animated && job.kind == GeneratedAssetKind.Human)
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();

            string textureFolder = job.AssetRoot + "/Textures";
            try
            {
                importer.ExtractTextures(AIAssetPipelinePaths.ToAbsolute(textureFolder));
                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[AI Asset Pipeline] Embedded texture extraction was skipped: " + exception.Message);
            }

            ExtractAndRemapMaterials(job, importer);
            importer.SaveAndReimport();

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(job.fbxAssetPath);
            if (modelAsset == null)
                throw new InvalidOperationException("Imported FBX has no GameObject root.");

            List<AnimationClip> clips = LoadAnimationClips(job.fbxAssetPath);
            if (animated && clips.Count == 0)
                throw new InvalidOperationException("Mesh2Motion FBX contains no usable animation clips.");

            Avatar avatar = null;
            if (animated)
            {
                avatar = AssetDatabase.LoadAllAssetsAtPath(job.fbxAssetPath).OfType<Avatar>().FirstOrDefault();
                if (job.kind == GeneratedAssetKind.Human && (avatar == null || !avatar.isValid || !avatar.isHuman))
                    throw new InvalidOperationException("Unity could not create a valid Humanoid avatar from the Mixamo-compatible skeleton.");
            }

            RuntimeAnimatorController controller = animated ? CreateAnimatorController(job, clips) : null;
            CreatePrefab(job, modelAsset, avatar, controller, animated);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureOutputFolders(GenerationJob job)
        {
            string[] folders = { "Source", "Models", "Textures", "Materials", "Animations", "Prefabs" };
            Directory.CreateDirectory(job.AbsoluteAssetRoot);
            foreach (string folder in folders)
                Directory.CreateDirectory(Path.Combine(job.AbsoluteAssetRoot, folder));
            AssetDatabase.Refresh();
        }

        private static void ExtractAndRemapMaterials(GenerationJob job, ModelImporter importer)
        {
            string materialFolder = job.AssetRoot + "/Materials";
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
                throw new InvalidOperationException("URP/Lit shader was not found.");

            Material[] embedded = AssetDatabase.LoadAllAssetsAtPath(job.fbxAssetPath)
                .OfType<Material>()
                .ToArray();

            foreach (Material source in embedded)
            {
                string safeName = AIAssetPipelinePaths.SanitizeName(source.name).Replace('-', '_');
                string materialPath = AssetDatabase.GenerateUniqueAssetPath(materialFolder + "/" + safeName + ".mat");
                var material = new Material(urpShader) { name = source.name };

                Texture baseTexture = source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
                Color baseColor = source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", baseTexture);
                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", baseColor);
                if (source.HasProperty("_BumpMap") && material.HasProperty("_BumpMap"))
                {
                    material.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
                    if (source.GetTexture("_BumpMap") != null)
                        material.EnableKeyword("_NORMALMAP");
                }
                if (source.HasProperty("_Metallic") && material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", source.GetFloat("_Metallic"));
                if (source.HasProperty("_Glossiness") && material.HasProperty("_Smoothness"))
                    material.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));

                AssetDatabase.CreateAsset(material, materialPath);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(source), material);
            }
        }

        private static List<AnimationClip> LoadAnimationClips(string fbxPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase) && clip.length > 0.01f)
                .OrderBy(clip => clip.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static RuntimeAnimatorController CreateAnimatorController(GenerationJob job, IList<AnimationClip> clips)
        {
            string controllerPath = job.AssetRoot + "/Animations/" + job.slug + ".controller";
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath) != null)
                AssetDatabase.DeleteAsset(controllerPath);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState first = null;
            foreach (AnimationClip clip in clips)
            {
                AnimatorState state = stateMachine.AddState(clip.name);
                state.motion = clip;
                if (first == null)
                    first = state;
            }
            if (first != null)
                stateMachine.defaultState = first;

            job.animatorControllerAssetPath = controllerPath;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void CreatePrefab(
            GenerationJob job,
            GameObject modelAsset,
            Avatar avatar,
            RuntimeAnimatorController controller,
            bool animated)
        {
            string prefabPath = job.AssetRoot + "/Prefabs/" + job.slug + ".prefab";
            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (instance == null)
                instance = UnityEngine.Object.Instantiate(modelAsset);

            try
            {
                instance.name = job.assetName;
                Animator animator = instance.GetComponent<Animator>();
                if (animated)
                {
                    if (animator == null)
                        animator = instance.AddComponent<Animator>();
                    animator.avatar = avatar;
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                }
                else if (animator != null)
                {
                    UnityEngine.Object.DestroyImmediate(animator);
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                job.prefabAssetPath = prefabPath;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
    }
}
