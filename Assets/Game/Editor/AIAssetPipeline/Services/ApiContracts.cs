using System;

namespace Game.Editor.AIAssetPipeline
{
    [Serializable]
    internal sealed class OpenAIImageRequest
    {
        public string model = "gpt-image-2.5-sunburst";
        public string prompt;
        public string size = "1024x1024";
        public string quality = "medium";
        public string background = "transparent";
        public string output_format = "png";
        public int n = 1;
    }

    [Serializable]
    internal sealed class OpenAIImageResponse
    {
        public OpenAIImageData[] data;
    }

    [Serializable]
    internal sealed class OpenAIImageData
    {
        public string b64_json;
        public string revised_prompt;
    }

    [Serializable]
    internal sealed class TripoImageToModelRequest
    {
        public string input;
        public string model = "v3.1-20260211";
        public bool texture = true;
        public bool pbr = true;
        public string texture_quality = "standard";
        public bool auto_size = true;
        public int face_limit;
    }

    [Serializable]
    internal sealed class TripoInputRequest
    {
        public string input;
    }

    [Serializable]
    internal sealed class TripoRigRequest
    {
        public string input;
        public string model = "v1.0-20240301";
        public string rig_type = "biped";
        public string spec = "mixamo";
        public string out_format = "glb";
    }

    [Serializable]
    internal sealed class TripoResponse
    {
        public int code;
        public TripoData data;
        public string message;
    }

    [Serializable]
    internal sealed class TripoData
    {
        public string file_token;
        public string task_id;
        public string type;
        public string status;
        public float progress;
        public float credits_consumed;
        public TripoOutput output;
    }

    [Serializable]
    internal sealed class TripoOutput
    {
        public string model_url;
        public string pbr_model;
        public string base_model;
        public string rendered_image_url;
        public bool riggable;
        public string rig_type;
    }
}
