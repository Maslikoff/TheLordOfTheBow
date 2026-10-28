using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline
{
    public enum GeneratedAssetKind
    {
        Human,
        Animal,
        Prop
    }

    public enum GenerationStage
    {
        Draft,
        GeneratingImage,
        AwaitingImageApproval,
        UploadingImage,
        GeneratingModel,
        CheckingRig,
        RiggingHuman,
        DownloadingModel,
        AwaitingMesh2Motion,
        Converting,
        Importing,
        Complete,
        Failed,
        Cancelled,
        NeedsAttention
    }

    [Serializable]
    public sealed class GenerationJob
    {
        public string id;
        public string assetName;
        public string slug;
        [TextArea(3, 8)] public string description;
        public GeneratedAssetKind kind;
        public List<string> requestedAnimations = new List<string>();
        public List<string> checkedAnimations = new List<string>();
        public GenerationStage stage = GenerationStage.Draft;
        public GenerationStage failedFromStage = GenerationStage.Draft;
        public float progress;
        public string statusMessage;
        public string error;

        public string effectivePrompt;
        public string openAiImageModel = "gpt-image-2.5-sunburst";
        public string openAiImageSize = "1024x1024";
        public string openAiImageQuality = "medium";
        public string openAiOutputFormat = "png";
        public bool openAiTransparentBackground = true;
        public string previewImagePath;
        public bool previewHasTransparency;
        public float transparentBorderRatio;
        public string acceptedImageAssetPath;

        public string tripoUploadToken;
        public string tripoModelVersion = "v3.1-20260211";
        public string tripoTextureQuality = "standard";
        public bool tripoTextures = true;
        public bool tripoPbr = true;
        public bool tripoAutoSize = true;
        public int tripoFaceLimit;
        public string tripoRigModelVersion = "v1.0-20240301";
        public string tripoRigType = "biped";
        public string tripoRigSpec = "mixamo";
        public string tripoRigOutputFormat = "glb";
        public string tripoModelTaskId;
        public string tripoRigCheckTaskId;
        public string tripoRigTaskId;
        public string tripoModelUrl;
        public string tripoRiggedModelUrl;
        public float tripoCreditsConsumed;

        public string sourceGlbAssetPath;
        public string mesh2MotionGlbAssetPath;
        public string fbxAssetPath;
        public string animatorControllerAssetPath;
        public string prefabAssetPath;

        public string createdUtc;
        public string updatedUtc;
        public string completedUtc;

        public bool HasRequestedAnimations
        {
            get { return requestedAnimations != null && requestedAnimations.Any(value => !string.IsNullOrWhiteSpace(value)); }
        }

        public bool RequiresMesh2Motion
        {
            get { return HasRequestedAnimations && kind != GeneratedAssetKind.Prop; }
        }

        public string AssetRoot
        {
            get { return "Assets/Game/Generated/" + slug; }
        }

        public string AbsoluteAssetRoot
        {
            get { return AIAssetPipelinePaths.ToAbsolute(AssetRoot); }
        }

        public static GenerationJob Create(string name, string description, GeneratedAssetKind kind, string animationText)
        {
            string now = DateTime.UtcNow.ToString("O");
            return new GenerationJob
            {
                id = Guid.NewGuid().ToString("N"),
                assetName = name.Trim(),
                slug = AIAssetPipelinePaths.SanitizeName(name),
                description = description.Trim(),
                kind = kind,
                requestedAnimations = ParseAnimations(animationText),
                stage = GenerationStage.Draft,
                createdUtc = now,
                updatedUtc = now,
                statusMessage = "Ready"
            };
        }

        public static List<string> ParseAnimations(string animationText)
        {
            if (string.IsNullOrWhiteSpace(animationText))
                return new List<string>();

            return animationText
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string AnimationsAsText()
        {
            return requestedAnimations == null ? string.Empty : string.Join("\n", requestedAnimations);
        }

        public void Touch()
        {
            updatedUtc = DateTime.UtcNow.ToString("O");
        }
    }

    public static class AIAssetPipelinePaths
    {
        public static string ProjectRoot
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
        }

        public static string StateDirectory
        {
            get { return Path.Combine(ProjectRoot, "Library", "AIAssetPipeline"); }
        }

        public static string ActiveJobPath
        {
            get { return Path.Combine(StateDirectory, "active-job.json"); }
        }

        public static string PreviewPath
        {
            get { return Path.Combine(StateDirectory, "preview.png"); }
        }

        public static string ToAbsolute(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(ProjectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        public static string ToAssetPath(string absolutePath)
        {
            string normalizedRoot = ProjectRoot.Replace('\\', '/').TrimEnd('/');
            string normalized = Path.GetFullPath(absolutePath).Replace('\\', '/');
            if (!normalized.StartsWith(normalizedRoot + "/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path is outside the Unity project: " + absolutePath);
            return normalized.Substring(normalizedRoot.Length + 1);
        }

        public static string SanitizeName(string value)
        {
            string source = string.IsNullOrWhiteSpace(value) ? "generated-asset" : value.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new string(source.Select(character => invalid.Contains(character) ? '-' : character).ToArray());
            cleaned = cleaned.Replace(' ', '-').Trim('-', '.').ToLowerInvariant();
            while (cleaned.Contains("--"))
                cleaned = cleaned.Replace("--", "-");
            return string.IsNullOrEmpty(cleaned) ? "generated-asset" : cleaned;
        }
    }
}
