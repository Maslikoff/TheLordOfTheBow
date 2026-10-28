using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline
{
    [InitializeOnLoad]
    public static class AIAssetPipelineController
    {
        private static CancellationTokenSource _cancellation;
        private static bool _running;

        public static GenerationJob Current { get; private set; }
        public static bool IsRunning { get { return _running; } }
        public static event Action Changed;

        static AIAssetPipelineController()
        {
            Current = GenerationJobStore.Load();
            EditorApplication.delayCall += TryAutoResume;
        }

        public static GenerationJob CreateJob(string name, string description, GeneratedAssetKind kind, string animations)
        {
            if (_running)
                throw new InvalidOperationException("Another AI asset job is still running.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Asset name is required.");
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");
            if (kind == GeneratedAssetKind.Prop && GenerationJob.ParseAnimations(animations).Count > 0)
                throw new ArgumentException("Props cannot request character animations.");

            GenerationJobStore.Clear();
            Current = GenerationJob.Create(name, description, kind, animations);
            SaveAndNotify();
            return Current;
        }

        public static async void GenerateImage()
        {
            if (!CanBeginOperation())
                return;

            BeginOperation();
            try
            {
                Current.effectivePrompt = PromptBuilder.Build(Current.description, Current.kind);
                SetStage(GenerationStage.GeneratingImage, "Generating transparent PNG with OpenAI...", 0.05f);
                byte[] png = await new OpenAIImageService().GenerateAsync(Current.effectivePrompt, _cancellation.Token);

                Directory.CreateDirectory(AIAssetPipelinePaths.StateDirectory);
                File.WriteAllBytes(AIAssetPipelinePaths.PreviewPath, png);
                TransparencyValidationResult validation = TransparencyValidator.ValidatePng(png);
                Current.previewImagePath = AIAssetPipelinePaths.PreviewPath;
                Current.previewHasTransparency = validation.IsValid;
                Current.transparentBorderRatio = validation.TransparentBorderRatio;
                Current.error = validation.IsValid ? string.Empty : validation.Message;
                SetStage(GenerationStage.AwaitingImageApproval, validation.Message, 1f);
            }
            catch (OperationCanceledException)
            {
                SetStage(GenerationStage.Cancelled, "Cancelled.", 0f);
            }
            catch (Exception exception)
            {
                HandleFailure(exception, "Image generation failed");
            }
            finally
            {
                EndOperation();
            }
        }

        public static async void ApproveImageAndGenerateModel()
        {
            if (!CanBeginOperation() || Current.stage != GenerationStage.AwaitingImageApproval)
                return;

            byte[] png;
            try
            {
                png = File.ReadAllBytes(Current.previewImagePath);
            }
            catch (Exception exception)
            {
                HandleFailure(exception, "Could not read the approved PNG");
                return;
            }

            TransparencyValidationResult validation = TransparencyValidator.ValidatePng(png);
            if (!validation.IsValid)
            {
                Current.error = validation.Message;
                SaveAndNotify();
                return;
            }

            BeginOperation();
            try
            {
                EnsureAssetDirectories();
                string acceptedPath = Current.AssetRoot + "/Source/" + Current.slug + "_concept.png";
                File.WriteAllBytes(AIAssetPipelinePaths.ToAbsolute(acceptedPath), png);
                Current.acceptedImageAssetPath = acceptedPath;
                AssetDatabase.ImportAsset(acceptedPath, ImportAssetOptions.ForceUpdate);

                var tripo = new TripoService();
                SetStage(GenerationStage.UploadingImage, "Uploading approved PNG to Tripo...", 0.05f);
                Current.tripoUploadToken = await tripo.UploadPngAsync(png, Current.slug + ".png", _cancellation.Token);
                SaveAndNotify();

                SetStage(GenerationStage.GeneratingModel, "Starting Tripo image-to-model task...", 0.1f);
                int faceLimit = Current.kind == GeneratedAssetKind.Prop ? 10000 : 20000;
                Current.tripoFaceLimit = faceLimit;
                SaveAndNotify();
                Current.tripoModelTaskId = await tripo.CreateModelAsync(Current.tripoUploadToken, faceLimit, _cancellation.Token);
                SaveAndNotify();

                TripoData model = await WaitForTripoTask(tripo, Current.tripoModelTaskId, "Generating 3D model");
                await ContinueAfterModelAsync(tripo, model, _cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                SetStage(GenerationStage.Cancelled, "Cancelled.", 0f);
            }
            catch (Exception exception)
            {
                HandleFailure(exception, "3D generation failed");
            }
            finally
            {
                EndOperation();
            }
        }

        public static void OpenMesh2Motion()
        {
            if (Current == null || Current.stage != GenerationStage.AwaitingMesh2Motion)
                return;
            Application.OpenURL("https://app.mesh2motion.org/create");
            string path = Current.sourceGlbAssetPath;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(AIAssetPipelinePaths.ToAbsolute(path)))
                EditorUtility.RevealInFinder(AIAssetPipelinePaths.ToAbsolute(path));
        }

        public static async void ImportMesh2MotionOutput(string selectedPath)
        {
            if (!CanBeginOperation() || Current.stage != GenerationStage.AwaitingMesh2Motion)
                return;
            if (string.IsNullOrWhiteSpace(selectedPath))
                return;
            if (!string.Equals(Path.GetExtension(selectedPath), ".glb", StringComparison.OrdinalIgnoreCase))
            {
                Current.error = "Mesh2Motion output must be a .glb file.";
                SaveAndNotify();
                return;
            }

            BeginOperation();
            try
            {
                EnsureAssetDirectories();
                string destination = Current.AssetRoot + "/Models/" + Current.slug + "_mesh2motion.glb";
                File.Copy(selectedPath, AIAssetPipelinePaths.ToAbsolute(destination), true);
                Current.mesh2MotionGlbAssetPath = destination;
                Current.sourceGlbAssetPath = destination;
                SaveAndNotify();
                await ConvertAndImportAsync(destination, _cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                SetStage(GenerationStage.Cancelled, "Cancelled.", 0f);
            }
            catch (Exception exception)
            {
                HandleFailure(exception, "Mesh2Motion import failed");
            }
            finally
            {
                EndOperation();
            }
        }

        public static void Cancel()
        {
            if (Current != null && Current.stage != GenerationStage.Cancelled)
            {
                Current.failedFromStage = Current.stage;
                SaveAndNotify();
            }
            if (_running && _cancellation != null)
                _cancellation.Cancel();
            else if (Current != null)
                SetStage(GenerationStage.Cancelled, "Cancelled.", 0f);
        }

        public static void ResetJob()
        {
            if (_running)
                return;
            GenerationJobStore.Clear();
            Current = null;
            Changed?.Invoke();
        }

        public static void RetryFailedStep()
        {
            if (Current == null || _running || Current.stage == GenerationStage.NeedsAttention)
                return;
            if (Current.stage != GenerationStage.Failed && Current.stage != GenerationStage.Cancelled)
                return;

            Current.error = string.Empty;
            Current.stage = Current.failedFromStage;
            SaveAndNotify();
            if (Current.stage == GenerationStage.GeneratingImage || Current.stage == GenerationStage.Draft)
            {
                GenerateImage();
                return;
            }
            if (Current.stage == GenerationStage.UploadingImage ||
                (Current.stage == GenerationStage.GeneratingModel && string.IsNullOrEmpty(Current.tripoModelTaskId)))
            {
                Current.stage = GenerationStage.AwaitingImageApproval;
                SaveAndNotify();
                ApproveImageAndGenerateModel();
                return;
            }
            Resume();
        }

        public static void RecoverWithTripoTaskId(string taskId)
        {
            if (Current == null || _running || Current.stage != GenerationStage.NeedsAttention)
                return;
            string normalized = (taskId ?? string.Empty).Trim();
            if (!normalized.StartsWith("task_", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Enter a Tripo task ID beginning with 'task_'.");

            switch (Current.failedFromStage)
            {
                case GenerationStage.GeneratingModel:
                    Current.tripoModelTaskId = normalized;
                    break;
                case GenerationStage.CheckingRig:
                    Current.tripoRigCheckTaskId = normalized;
                    break;
                case GenerationStage.RiggingHuman:
                    Current.tripoRigTaskId = normalized;
                    break;
                default:
                    throw new InvalidOperationException("This interrupted operation cannot be recovered with a Tripo task ID.");
            }

            Current.error = string.Empty;
            Current.stage = Current.failedFromStage;
            SaveAndNotify();
            Resume();
        }

        public static void StartNewImageRequestAfterAmbiguousFailure()
        {
            if (Current == null || _running || Current.stage != GenerationStage.NeedsAttention ||
                Current.failedFromStage != GenerationStage.GeneratingImage)
                return;
            Current.error = string.Empty;
            Current.stage = GenerationStage.Draft;
            SaveAndNotify();
            GenerateImage();
        }

        public static void SetAnimationChecklistState(string animationName, bool isChecked)
        {
            if (Current == null || string.IsNullOrWhiteSpace(animationName))
                return;
            if (Current.checkedAnimations == null)
                Current.checkedAnimations = new System.Collections.Generic.List<string>();

            int index = Current.checkedAnimations.FindIndex(
                value => string.Equals(value, animationName, StringComparison.OrdinalIgnoreCase));
            if (isChecked && index < 0)
                Current.checkedAnimations.Add(animationName);
            else if (!isChecked && index >= 0)
                Current.checkedAnimations.RemoveAt(index);
            SaveAndNotify();
        }

        public static async void Resume()
        {
            if (!CanBeginOperation())
                return;
            BeginOperation();
            try
            {
                await ResumeInternalAsync(_cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                SetStage(GenerationStage.Cancelled, "Cancelled.", 0f);
            }
            catch (Exception exception)
            {
                HandleFailure(exception, "Resume failed");
            }
            finally
            {
                EndOperation();
            }
        }

        private static async Task ResumeInternalAsync(CancellationToken cancellationToken)
        {
            switch (Current.stage)
            {
                case GenerationStage.GeneratingModel:
                    if (string.IsNullOrEmpty(Current.tripoModelTaskId))
                        throw new PipelineApiException(
                            "The paid model request has no saved task ID. Find it in Tripo and paste it into the recovery field.",
                            0,
                            true);
                    var tripoForModel = new TripoService();
                    TripoData model = await WaitForTripoTask(tripoForModel, Current.tripoModelTaskId, "Resuming 3D model generation");
                    await ContinueAfterModelAsync(tripoForModel, model, cancellationToken);
                    return;

                case GenerationStage.CheckingRig:
                    if (string.IsNullOrEmpty(Current.tripoRigCheckTaskId))
                        throw new PipelineApiException(
                            "The rig-check request has no saved task ID. Find it in Tripo and paste it into the recovery field.",
                            0,
                            true);
                    var tripoForCheck = new TripoService();
                    TripoData rigCheck = await WaitForTripoTask(tripoForCheck, Current.tripoRigCheckTaskId, "Resuming rig check");
                    await ContinueAfterRigCheckAsync(tripoForCheck, rigCheck, cancellationToken);
                    return;

                case GenerationStage.RiggingHuman:
                    if (string.IsNullOrEmpty(Current.tripoRigTaskId))
                        throw new PipelineApiException(
                            "The paid rig request has no saved task ID. Find it in Tripo and paste it into the recovery field.",
                            0,
                            true);
                    var tripoForRig = new TripoService();
                    TripoData rig = await WaitForTripoTask(tripoForRig, Current.tripoRigTaskId, "Resuming human rig");
                    await FinishHumanRigAsync(tripoForRig, rig, cancellationToken);
                    return;

                case GenerationStage.DownloadingModel:
                    var tripoForDownload = new TripoService();
                    string url = !string.IsNullOrEmpty(Current.tripoRiggedModelUrl)
                        ? Current.tripoRiggedModelUrl
                        : Current.tripoModelUrl;
                    await DownloadForNextStepAsync(tripoForDownload, url, Current.RequiresMesh2Motion, cancellationToken);
                    return;

                case GenerationStage.Converting:
                case GenerationStage.Importing:
                    if (string.IsNullOrEmpty(Current.sourceGlbAssetPath))
                        throw new InvalidOperationException("The source GLB path is missing.");
                    await ConvertAndImportAsync(Current.sourceGlbAssetPath, cancellationToken);
                    return;

                default:
                    throw new InvalidOperationException("This stage does not require resuming: " + Current.stage);
            }
        }

        private static async Task ContinueAfterModelAsync(TripoService tripo, TripoData model, CancellationToken cancellationToken)
        {
            Current.tripoCreditsConsumed += model.credits_consumed;
            Current.tripoModelUrl = SelectModelUrl(model);
            SaveAndNotify();

            if (!Current.RequiresMesh2Motion)
            {
                await DownloadForNextStepAsync(tripo, Current.tripoModelUrl, false, cancellationToken);
                return;
            }

            if (Current.kind == GeneratedAssetKind.Animal)
            {
                await DownloadForNextStepAsync(tripo, Current.tripoModelUrl, true, cancellationToken);
                return;
            }

            SetStage(GenerationStage.CheckingRig, "Checking whether the human model can be rigged...", 0.1f);
            Current.tripoRigCheckTaskId = await tripo.CreateRigCheckAsync(Current.tripoModelTaskId, cancellationToken);
            SaveAndNotify();
            TripoData rigCheck = await WaitForTripoTask(tripo, Current.tripoRigCheckTaskId, "Checking rig compatibility");
            await ContinueAfterRigCheckAsync(tripo, rigCheck, cancellationToken);
        }

        private static async Task ContinueAfterRigCheckAsync(TripoService tripo, TripoData rigCheck, CancellationToken cancellationToken)
        {
            Current.tripoCreditsConsumed += rigCheck.credits_consumed;
            if (rigCheck.output == null || !rigCheck.output.riggable)
                throw new InvalidOperationException("Tripo reports that the human model is not riggable. Regenerate the image/model.");
            if (!string.IsNullOrEmpty(rigCheck.output.rig_type) &&
                !string.Equals(rigCheck.output.rig_type, "biped", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Tripo recommended rig type '" + rigCheck.output.rig_type + "' instead of biped.");

            SetStage(GenerationStage.RiggingHuman, "Creating Mixamo-compatible biped rig...", 0.1f);
            Current.tripoRigTaskId = await tripo.CreateHumanRigAsync(Current.tripoModelTaskId, cancellationToken);
            SaveAndNotify();
            TripoData rig = await WaitForTripoTask(tripo, Current.tripoRigTaskId, "Creating Mixamo-compatible rig");
            await FinishHumanRigAsync(tripo, rig, cancellationToken);
        }

        private static async Task FinishHumanRigAsync(TripoService tripo, TripoData rig, CancellationToken cancellationToken)
        {
            Current.tripoCreditsConsumed += rig.credits_consumed;
            Current.tripoRiggedModelUrl = SelectModelUrl(rig);
            SaveAndNotify();
            await DownloadForNextStepAsync(tripo, Current.tripoRiggedModelUrl, true, cancellationToken);
        }

        private static async Task DownloadForNextStepAsync(
            TripoService tripo,
            string url,
            bool mesh2Motion,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("Tripo task completed without a model URL.");

            SetStage(GenerationStage.DownloadingModel, "Downloading generated GLB...", 0.2f);
            byte[] bytes = await tripo.DownloadAsync(url, cancellationToken);
            EnsureAssetDirectories();
            string suffix = Current.kind == GeneratedAssetKind.Human && mesh2Motion ? "_mixamo" : "_tripo";
            string assetPath = Current.AssetRoot + "/Models/" + Current.slug + suffix + ".glb";
            File.WriteAllBytes(AIAssetPipelinePaths.ToAbsolute(assetPath), bytes);
            Current.sourceGlbAssetPath = assetPath;
            AssetDatabase.Refresh();
            SaveAndNotify();

            if (mesh2Motion)
            {
                SetStage(
                    GenerationStage.AwaitingMesh2Motion,
                    Current.kind == GeneratedAssetKind.Human
                        ? "Open the Mixamo-rigged GLB in Mesh2Motion, select animations, and export one GLB."
                        : "Open the GLB in Mesh2Motion, choose an animal skeleton, select animations, and export one GLB.",
                    1f);
                return;
            }

            await ConvertAndImportAsync(assetPath, cancellationToken);
        }

        private static async Task ConvertAndImportAsync(string sourceGlbAssetPath, CancellationToken cancellationToken)
        {
            SetStage(GenerationStage.Converting, "Converting GLB to FBX with Blender...", 0.3f);
            string fbxPath = Current.AssetRoot + "/Models/" + Current.slug + ".fbx";
            await BlenderConverter.ConvertAsync(
                AIAssetPipelinePaths.ToAbsolute(sourceGlbAssetPath),
                AIAssetPipelinePaths.ToAbsolute(fbxPath),
                cancellationToken);

            Current.fbxAssetPath = fbxPath;
            AssetDatabase.Refresh();
            SetStage(GenerationStage.Importing, "Configuring Unity model, materials, animations, and prefab...", 0.75f);
            UnityGeneratedAssetImporter.ImportAndCreatePrefab(Current);
            Current.completedUtc = DateTime.UtcNow.ToString("O");
            Current.error = string.Empty;
            SetStage(GenerationStage.Complete, "Prefab created successfully.", 1f);
            GenerationJobStore.SaveManifest(Current);

            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Current.prefabAssetPath);
            if (prefab != null)
                EditorGUIUtility.PingObject(prefab);
        }

        private static async Task<TripoData> WaitForTripoTask(TripoService tripo, string taskId, string label)
        {
            return await tripo.WaitForTaskAsync(
                taskId,
                (progress, status) =>
                {
                    Current.progress = progress;
                    Current.statusMessage = label + ": " + status + " " + Mathf.RoundToInt(progress * 100f) + "%";
                    SaveAndNotify();
                },
                _cancellation.Token);
        }

        private static string SelectModelUrl(TripoData data)
        {
            if (data == null || data.output == null)
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(data.output.pbr_model))
                return data.output.pbr_model;
            if (!string.IsNullOrWhiteSpace(data.output.model_url))
                return data.output.model_url;
            return data.output.base_model;
        }

        private static void EnsureAssetDirectories()
        {
            Directory.CreateDirectory(Current.AbsoluteAssetRoot);
            foreach (string folder in new[] { "Source", "Models", "Textures", "Materials", "Animations", "Prefabs" })
                Directory.CreateDirectory(Path.Combine(Current.AbsoluteAssetRoot, folder));
        }

        private static bool CanBeginOperation()
        {
            return Current != null && !_running;
        }

        private static void BeginOperation()
        {
            _running = true;
            _cancellation = new CancellationTokenSource();
            Changed?.Invoke();
        }

        private static void EndOperation()
        {
            _running = false;
            if (_cancellation != null)
            {
                _cancellation.Dispose();
                _cancellation = null;
            }
            Changed?.Invoke();
        }

        private static void SetStage(GenerationStage stage, string message, float progress)
        {
            Current.stage = stage;
            Current.statusMessage = message;
            Current.progress = Mathf.Clamp01(progress);
            if (stage != GenerationStage.Failed && stage != GenerationStage.NeedsAttention)
                Current.error = string.Empty;
            SaveAndNotify();
        }

        private static void HandleFailure(Exception exception, string context)
        {
            var apiException = exception as PipelineApiException;
            bool ambiguous = apiException != null && apiException.PaidPostMayHaveSucceeded;
            Current.failedFromStage = Current.stage;
            Current.stage = ambiguous ? GenerationStage.NeedsAttention : GenerationStage.Failed;
            Current.statusMessage = ambiguous
                ? context + ": server acceptance is unknown; the paid request was not repeated."
                : context + ".";
            Current.error = exception.Message;
            SaveAndNotify();
            UnityEngine.Debug.LogException(exception);
        }

        private static void SaveAndNotify()
        {
            GenerationJobStore.Save(Current);
            Changed?.Invoke();
        }

        private static void TryAutoResume()
        {
            if (Current == null || _running)
                return;
            switch (Current.stage)
            {
                case GenerationStage.GeneratingModel:
                case GenerationStage.CheckingRig:
                case GenerationStage.RiggingHuman:
                case GenerationStage.DownloadingModel:
                case GenerationStage.Converting:
                case GenerationStage.Importing:
                    Resume();
                    break;
                case GenerationStage.GeneratingImage:
                    Current.failedFromStage = Current.stage;
                    Current.stage = GenerationStage.NeedsAttention;
                    Current.statusMessage = "The editor reloaded while the paid image request was in flight. It was not repeated.";
                    SaveAndNotify();
                    break;
                case GenerationStage.UploadingImage:
                    Current.stage = GenerationStage.AwaitingImageApproval;
                    Current.statusMessage = "The editor reloaded during the non-billable upload. Approve the PNG to restart the Tripo handoff.";
                    SaveAndNotify();
                    break;
            }
        }
    }
}
