using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline
{
    public static class GenerationJobStore
    {
        public static GenerationJob Load()
        {
            try
            {
                if (!File.Exists(AIAssetPipelinePaths.ActiveJobPath))
                    return null;
                return JsonUtility.FromJson<GenerationJob>(File.ReadAllText(AIAssetPipelinePaths.ActiveJobPath));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[AI Asset Pipeline] Could not restore active job: " + exception.Message);
                return null;
            }
        }

        public static void Save(GenerationJob job)
        {
            if (job == null)
                return;

            Directory.CreateDirectory(AIAssetPipelinePaths.StateDirectory);
            job.Touch();
            string json = JsonUtility.ToJson(job, true);
            string temporaryPath = AIAssetPipelinePaths.ActiveJobPath + ".tmp";
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(AIAssetPipelinePaths.ActiveJobPath))
                File.Delete(AIAssetPipelinePaths.ActiveJobPath);
            File.Move(temporaryPath, AIAssetPipelinePaths.ActiveJobPath);
        }

        public static void SaveManifest(GenerationJob job)
        {
            if (job == null || string.IsNullOrEmpty(job.AssetRoot))
                return;

            Directory.CreateDirectory(job.AbsoluteAssetRoot);
            string manifestPath = Path.Combine(job.AbsoluteAssetRoot, "generation-manifest.json");
            File.WriteAllText(manifestPath, JsonUtility.ToJson(job, true));
            AssetDatabase.ImportAsset(AIAssetPipelinePaths.ToAssetPath(manifestPath), ImportAssetOptions.ForceUpdate);
        }

        public static void Clear()
        {
            if (File.Exists(AIAssetPipelinePaths.ActiveJobPath))
                File.Delete(AIAssetPipelinePaths.ActiveJobPath);
            if (File.Exists(AIAssetPipelinePaths.PreviewPath))
                File.Delete(AIAssetPipelinePaths.PreviewPath);
        }
    }
}
