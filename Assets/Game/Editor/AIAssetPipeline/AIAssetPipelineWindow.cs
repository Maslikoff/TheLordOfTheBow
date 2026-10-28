using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline
{
    public sealed class AIAssetPipelineWindow : EditorWindow
    {
        private string _assetName = string.Empty;
        private string _description = string.Empty;
        private GeneratedAssetKind _kind;
        private string _animations = string.Empty;
        private Vector2 _scroll;
        private Texture2D _preview;
        private string _loadedPreviewPath;
        private bool _settingsExpanded;
        private string _recoveryTaskId = string.Empty;

        [MenuItem("Game/AI Asset Pipeline")]
        public static void Open()
        {
            var window = GetWindow<AIAssetPipelineWindow>("AI Asset Pipeline");
            window.minSize = new Vector2(520f, 640f);
            window.Show();
        }

        private void OnEnable()
        {
            AIAssetPipelineController.Changed += OnPipelineChanged;
            ReloadPreview();
        }

        private void OnDisable()
        {
            AIAssetPipelineController.Changed -= OnPipelineChanged;
            DestroyPreview();
        }

        private void OnPipelineChanged()
        {
            ReloadPreview();
            Repaint();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            DrawSettings();
            EditorGUILayout.Space(8f);

            GenerationJob job = AIAssetPipelineController.Current;
            if (job == null)
                DrawNewJobForm();
            else
                DrawJob(job);

            EditorGUILayout.EndScrollView();
        }

        private static void DrawHeader()
        {
            EditorGUILayout.LabelField("AI Asset Pipeline", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Prompt → transparent PNG → approval → Tripo → optional Mesh2Motion → FBX → prefab",
                MessageType.Info);

            bool openAi = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            bool tripo = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TRIPO_API_KEY"));
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("OPENAI_API_KEY", openAi ? "Available" : "Missing");
                EditorGUILayout.LabelField("TRIPO_API_KEY", tripo ? "Available" : "Missing");
            }
            if (!openAi || !tripo)
                EditorGUILayout.HelpBox("Define the missing environment variables, then restart Unity. Keys are never saved in the project.", MessageType.Warning);
        }

        private void DrawSettings()
        {
            _settingsExpanded = EditorGUILayout.Foldout(_settingsExpanded, "Local settings", true);
            if (!_settingsExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                string blenderPath = BlenderConverter.BlenderPath;
                using (new EditorGUILayout.HorizontalScope())
                {
                    string edited = EditorGUILayout.TextField("Blender", blenderPath);
                    if (!string.Equals(edited, blenderPath, StringComparison.Ordinal))
                        BlenderConverter.BlenderPath = edited;
                    if (GUILayout.Button("Browse", GUILayout.Width(70f)))
                    {
                        string initialDirectory = string.IsNullOrWhiteSpace(blenderPath)
                            ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                            : Path.GetDirectoryName(blenderPath);
                        string selected = EditorUtility.OpenFilePanel("Select Blender executable", initialDirectory, "exe");
                        if (!string.IsNullOrWhiteSpace(selected))
                            BlenderConverter.BlenderPath = selected;
                    }
                }
                if (string.IsNullOrWhiteSpace(BlenderConverter.BlenderPath) || !File.Exists(BlenderConverter.BlenderPath))
                    EditorGUILayout.HelpBox("Blender was not found. GLB conversion cannot finish until the path is configured.", MessageType.Warning);
            }
        }

        private void DrawNewJobForm()
        {
            EditorGUILayout.LabelField("New asset", EditorStyles.boldLabel);
            _assetName = EditorGUILayout.TextField("Asset name", _assetName);
            _kind = (GeneratedAssetKind)EditorGUILayout.EnumPopup("Kind", _kind);
            EditorGUILayout.LabelField("Description");
            _description = EditorGUILayout.TextArea(_description, GUILayout.MinHeight(90f));

            using (new EditorGUI.DisabledScope(_kind == GeneratedAssetKind.Prop))
            {
                EditorGUILayout.LabelField("Requested animations (one per line, optional)");
                _animations = EditorGUILayout.TextArea(_kind == GeneratedAssetKind.Prop ? string.Empty : _animations, GUILayout.MinHeight(60f));
            }
            EditorGUILayout.HelpBox("An empty animation list produces a static model with no rig. Humans use a Mixamo-compatible Tripo rig; animals are rigged manually in Mesh2Motion.", MessageType.None);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_assetName) || string.IsNullOrWhiteSpace(_description)))
            {
                if (GUILayout.Button("Create job and generate PNG", GUILayout.Height(34f)))
                {
                    try
                    {
                        AIAssetPipelineController.CreateJob(_assetName, _description, _kind, _animations);
                        AIAssetPipelineController.GenerateImage();
                    }
                    catch (Exception exception)
                    {
                        EditorUtility.DisplayDialog("AI Asset Pipeline", exception.Message, "OK");
                    }
                }
            }
        }

        private void DrawJob(GenerationJob job)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(job.assetName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Kind", job.kind.ToString());
                EditorGUILayout.LabelField("Stage", job.stage.ToString());
                Rect progressRect = GUILayoutUtility.GetRect(10f, 20f, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(progressRect, Mathf.Clamp01(job.progress), job.statusMessage ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(job.error))
                    EditorGUILayout.HelpBox(job.error, job.stage == GenerationStage.NeedsAttention ? MessageType.Warning : MessageType.Error);
            }

            EditorGUILayout.Space(8f);
            if (job.stage == GenerationStage.AwaitingImageApproval)
                DrawImageApproval(job);
            else if (job.stage == GenerationStage.AwaitingMesh2Motion)
                DrawMesh2Motion(job);
            else if (job.stage == GenerationStage.Complete)
                DrawComplete(job);
            else if (job.stage == GenerationStage.Failed || job.stage == GenerationStage.Cancelled || job.stage == GenerationStage.NeedsAttention)
                DrawStopped(job);
            else
                DrawRunning(job);

            EditorGUILayout.Space(12f);
            DrawJobDetails(job);
        }

        private void DrawImageApproval(GenerationJob job)
        {
            EditorGUILayout.LabelField("PNG approval", EditorStyles.boldLabel);
            if (_preview != null)
            {
                float width = Mathf.Min(position.width - 48f, 480f);
                Rect rect = GUILayoutUtility.GetRect(width, width, GUILayout.ExpandWidth(false));
                EditorGUI.DrawPreviewTexture(rect, _preview, null, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUILayout.HelpBox("Preview file could not be loaded.", MessageType.Error);
            }

            EditorGUILayout.LabelField("Transparent border", job.transparentBorderRatio.ToString("P0"));
            if (!job.previewHasTransparency)
                EditorGUILayout.HelpBox("Approval is disabled until a PNG with a transparent border is generated.", MessageType.Warning);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(AIAssetPipelineController.IsRunning))
                {
                    if (GUILayout.Button("Regenerate PNG"))
                        AIAssetPipelineController.GenerateImage();
                }
                using (new EditorGUI.DisabledScope(AIAssetPipelineController.IsRunning || !job.previewHasTransparency))
                {
                    if (GUILayout.Button("Approve and start Tripo"))
                        AIAssetPipelineController.ApproveImageAndGenerateModel();
                }
                if (GUILayout.Button("Cancel"))
                    AIAssetPipelineController.Cancel();
            }
        }

        private static void DrawMesh2Motion(GenerationJob job)
        {
            EditorGUILayout.LabelField("Mesh2Motion handoff", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(job.statusMessage, MessageType.Info);
            if (job.requestedAnimations != null && job.requestedAnimations.Count > 0)
            {
                EditorGUILayout.LabelField("Requested animations:");
                foreach (string animationName in job.requestedAnimations)
                {
                    bool wasChecked = job.checkedAnimations != null && job.checkedAnimations.Exists(
                        value => string.Equals(value, animationName, StringComparison.OrdinalIgnoreCase));
                    bool isChecked = EditorGUILayout.ToggleLeft(animationName, wasChecked);
                    if (isChecked != wasChecked)
                        AIAssetPipelineController.SetAnimationChecklistState(animationName, isChecked);
                }
            }

            if (GUILayout.Button("Open Mesh2Motion and reveal source GLB", GUILayout.Height(30f)))
                AIAssetPipelineController.OpenMesh2Motion();
            if (GUILayout.Button("Import exported Mesh2Motion GLB", GUILayout.Height(30f)))
            {
                string selected = EditorUtility.OpenFilePanel("Select Mesh2Motion GLB", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "glb");
                if (!string.IsNullOrWhiteSpace(selected))
                    AIAssetPipelineController.ImportMesh2MotionOutput(selected);
            }
            if (GUILayout.Button("Cancel"))
                AIAssetPipelineController.Cancel();
        }

        private static void DrawComplete(GenerationJob job)
        {
            EditorGUILayout.HelpBox("The generated FBX, materials, animations and prefab are ready.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping prefab"))
                {
                    UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(job.prefabAssetPath);
                    if (prefab != null)
                        EditorGUIUtility.PingObject(prefab);
                }
                if (GUILayout.Button("Reveal asset folder"))
                    EditorUtility.RevealInFinder(job.AbsoluteAssetRoot);
                if (GUILayout.Button("New job"))
                    AIAssetPipelineController.ResetJob();
            }
        }

        private void DrawStopped(GenerationJob job)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (job.stage != GenerationStage.NeedsAttention && GUILayout.Button("Retry failed step"))
                    AIAssetPipelineController.RetryFailedStep();
                if (GUILayout.Button("Start a new job"))
                    AIAssetPipelineController.ResetJob();
            }
            if (job.stage == GenerationStage.NeedsAttention)
            {
                EditorGUILayout.HelpBox("A paid POST may have reached the server without returning a task ID. The tool deliberately will not repeat it automatically.", MessageType.Warning);
                if (CanRecoverWithTripoTaskId(job.failedFromStage))
                {
                    EditorGUILayout.LabelField("Manual Tripo recovery", EditorStyles.boldLabel);
                    _recoveryTaskId = EditorGUILayout.TextField("Task ID", _recoveryTaskId);
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_recoveryTaskId)))
                    {
                        if (GUILayout.Button("Resume from this Tripo task ID"))
                        {
                            try
                            {
                                AIAssetPipelineController.RecoverWithTripoTaskId(_recoveryTaskId);
                            }
                            catch (Exception exception)
                            {
                                EditorUtility.DisplayDialog("AI Asset Pipeline", exception.Message, "OK");
                            }
                        }
                    }
                    if (GUILayout.Button("Open Tripo Studio"))
                        Application.OpenURL("https://studio.tripo3d.ai/ru");
                }
                else if (job.failedFromStage == GenerationStage.GeneratingImage)
                {
                    EditorGUILayout.HelpBox("OpenAI image requests cannot be resumed by ID. Starting again creates a new paid request.", MessageType.Info);
                    if (GUILayout.Button("Generate a new PNG (may charge again)"))
                        AIAssetPipelineController.StartNewImageRequestAfterAmbiguousFailure();
                }
            }
        }

        private static bool CanRecoverWithTripoTaskId(GenerationStage stage)
        {
            return stage == GenerationStage.GeneratingModel ||
                   stage == GenerationStage.CheckingRig ||
                   stage == GenerationStage.RiggingHuman;
        }

        private static void DrawRunning(GenerationJob job)
        {
            using (new EditorGUI.DisabledScope(!AIAssetPipelineController.IsRunning))
            {
                if (GUILayout.Button("Cancel current operation"))
                    AIAssetPipelineController.Cancel();
            }
            if (!AIAssetPipelineController.IsRunning && IsResumable(job.stage) && GUILayout.Button("Resume"))
                AIAssetPipelineController.Resume();
        }

        private static bool IsResumable(GenerationStage stage)
        {
            return stage == GenerationStage.GeneratingModel ||
                   stage == GenerationStage.CheckingRig ||
                   stage == GenerationStage.RiggingHuman ||
                   stage == GenerationStage.DownloadingModel ||
                   stage == GenerationStage.Converting ||
                   stage == GenerationStage.Importing;
        }

        private static void DrawJobDetails(GenerationJob job)
        {
            EditorGUILayout.LabelField("Job details", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Description", job.description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Animations", string.IsNullOrWhiteSpace(job.AnimationsAsText()) ? "None (static)" : job.AnimationsAsText(), EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("Tripo credits reported", job.tripoCreditsConsumed.ToString("0.##"));
            if (!string.IsNullOrWhiteSpace(job.tripoModelTaskId))
                EditorGUILayout.SelectableLabel("Model task: " + job.tripoModelTaskId, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (!string.IsNullOrWhiteSpace(job.tripoRigTaskId))
                EditorGUILayout.SelectableLabel("Rig task: " + job.tripoRigTaskId, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        private void ReloadPreview()
        {
            GenerationJob job = AIAssetPipelineController.Current;
            string path = job == null ? string.Empty : job.previewImagePath;
            if (string.Equals(path, _loadedPreviewPath, StringComparison.OrdinalIgnoreCase) && _preview != null)
                return;

            DestroyPreview();
            _loadedPreviewPath = path;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (texture.LoadImage(File.ReadAllBytes(path), false))
                _preview = texture;
            else
                DestroyImmediate(texture);
        }

        private void DestroyPreview()
        {
            if (_preview != null)
                DestroyImmediate(_preview);
            _preview = null;
            _loadedPreviewPath = string.Empty;
        }
    }
}
