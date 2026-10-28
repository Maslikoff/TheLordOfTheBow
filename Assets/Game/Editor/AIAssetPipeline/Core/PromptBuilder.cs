using System;

namespace Game.Editor.AIAssetPipeline
{
    public static class PromptBuilder
    {
        private const string SharedProfile =
            "Create one production-ready stylized low-poly 3D game asset concept. " +
            "Show exactly one subject, fully visible and centered, with a clean readable silhouette and moderate detail. " +
            "Use a fully transparent background with clean alpha edges. " +
            "No floor, cast shadow, environment, scenery, text, watermark, border, base, or pedestal.";

        public static string Build(string description, GeneratedAssetKind kind)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.", nameof(description));

            string pose;
            switch (kind)
            {
                case GeneratedAssetKind.Human:
                    pose = "Full-body humanoid character in a strict front-facing T-pose: upright torso, arms straight and horizontal, palms down, legs straight and slightly apart. Do not crop hands, feet, or head.";
                    break;
                case GeneratedAssetKind.Animal:
                    pose = "Full-body animal standing naturally on all of its paws in a neutral idle pose, anatomically readable, with every limb visible. Do not crop ears, tail, or paws.";
                    break;
                default:
                    pose = "Show the object in its natural canonical upright orientation, unobstructed and fully inside the frame.";
                    break;
            }

            return description.Trim() + "\n\n" + pose + "\n" + SharedProfile;
        }
    }
}
