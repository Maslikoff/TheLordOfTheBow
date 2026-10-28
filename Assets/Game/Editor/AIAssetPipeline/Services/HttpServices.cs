using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.Editor.AIAssetPipeline
{
    public sealed class PipelineApiException : Exception
    {
        public long StatusCode { get; private set; }
        public bool PaidPostMayHaveSucceeded { get; private set; }

        public PipelineApiException(string message, long statusCode = 0, bool paidPostMayHaveSucceeded = false)
            : base(message)
        {
            StatusCode = statusCode;
            PaidPostMayHaveSucceeded = paidPostMayHaveSucceeded;
        }
    }

    internal static class UnityWebRequestAsync
    {
        public static async Task SendAsync(UnityWebRequest request, CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<bool>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += _ => completion.TrySetResult(true);
            if (operation.isDone)
                completion.TrySetResult(true);
            using (cancellationToken.Register(() =>
                   {
                       request.Abort();
                       completion.TrySetCanceled();
                   }))
            {
                await completion.Task;
            }
        }

        public static void EnsureSuccess(UnityWebRequest request, bool paidPost = false)
        {
            if (request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 300)
                return;

            string response = request.downloadHandler == null ? string.Empty : request.downloadHandler.text;
            string detail = string.IsNullOrWhiteSpace(response) ? request.error : response;
            bool ambiguous = paidPost && request.result == UnityWebRequest.Result.ConnectionError;
            string category;
            switch (request.responseCode)
            {
                case 401:
                case 403:
                    category = "Authorization failed. Check the API key and account access.";
                    break;
                case 402:
                    category = "The account has insufficient credits or billing is unavailable.";
                    break;
                case 429:
                    category = "The service rate limit was reached. Wait before retrying.";
                    break;
                case 0:
                    category = "The service could not be reached or the request timed out.";
                    break;
                default:
                    category = "The service rejected the request.";
                    break;
            }
            throw new PipelineApiException(
                string.Format("{0} HTTP {1}: {2}", category, request.responseCode, detail),
                request.responseCode,
                ambiguous);
        }

        public static UnityWebRequest JsonPost(string url, string apiKey, string json)
        {
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.timeout = 180;
            return request;
        }
    }

    internal sealed class OpenAIImageService
    {
        private const string Endpoint = "https://api.openai.com/v1/images/generations";

        public async Task<byte[]> GenerateAsync(string prompt, CancellationToken cancellationToken)
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new PipelineApiException("OPENAI_API_KEY is not set. Restart Unity after defining the environment variable.");

            var body = new OpenAIImageRequest { prompt = prompt };
            using (UnityWebRequest request = UnityWebRequestAsync.JsonPost(Endpoint, apiKey, JsonUtility.ToJson(body)))
            {
                await UnityWebRequestAsync.SendAsync(request, cancellationToken);
                UnityWebRequestAsync.EnsureSuccess(request, true);
                OpenAIImageResponse response = JsonUtility.FromJson<OpenAIImageResponse>(request.downloadHandler.text);
                if (response == null || response.data == null || response.data.Length == 0 || string.IsNullOrEmpty(response.data[0].b64_json))
                    throw new PipelineApiException("OpenAI returned no image data.");
                try
                {
                    return Convert.FromBase64String(response.data[0].b64_json);
                }
                catch (FormatException exception)
                {
                    throw new PipelineApiException("OpenAI returned invalid base64 image data: " + exception.Message);
                }
            }
        }
    }

    internal sealed class TripoService
    {
        private const string BaseUrl = "https://openapi.tripo3d.ai/v3";
        private readonly string _apiKey;

        public TripoService()
        {
            _apiKey = Environment.GetEnvironmentVariable("TRIPO_API_KEY");
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new PipelineApiException("TRIPO_API_KEY is not set. Restart Unity after defining the environment variable.");
        }

        public async Task<string> UploadPngAsync(byte[] pngBytes, string fileName, CancellationToken cancellationToken)
        {
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", pngBytes, fileName, "image/png")
            };
            using (UnityWebRequest request = UnityWebRequest.Post(BaseUrl + "/files", form))
            {
                request.SetRequestHeader("Authorization", "Bearer " + _apiKey);
                request.timeout = 180;
                await UnityWebRequestAsync.SendAsync(request, cancellationToken);
                UnityWebRequestAsync.EnsureSuccess(request);
                TripoResponse response = Parse(request.downloadHandler.text);
                if (response.data == null || string.IsNullOrEmpty(response.data.file_token))
                    throw new PipelineApiException("Tripo upload returned no file token.");
                return response.data.file_token;
            }
        }

        public Task<string> CreateModelAsync(string fileToken, int faceLimit, CancellationToken cancellationToken)
        {
            return CreatePaidTaskAsync(
                "/generation/image-to-model",
                new TripoImageToModelRequest { input = fileToken, face_limit = faceLimit },
                cancellationToken);
        }

        public Task<string> CreateRigCheckAsync(string modelTaskId, CancellationToken cancellationToken)
        {
            return CreatePaidTaskAsync("/animations/rig-check", new TripoInputRequest { input = modelTaskId }, cancellationToken);
        }

        public Task<string> CreateHumanRigAsync(string modelTaskId, CancellationToken cancellationToken)
        {
            return CreatePaidTaskAsync("/animations/rig", new TripoRigRequest { input = modelTaskId }, cancellationToken);
        }

        private async Task<string> CreatePaidTaskAsync(string route, object body, CancellationToken cancellationToken)
        {
            string json = JsonUtility.ToJson(body);
            using (UnityWebRequest request = UnityWebRequestAsync.JsonPost(BaseUrl + route, _apiKey, json))
            {
                await UnityWebRequestAsync.SendAsync(request, cancellationToken);
                UnityWebRequestAsync.EnsureSuccess(request, true);
                TripoResponse response = Parse(request.downloadHandler.text);
                if (response.data == null || string.IsNullOrEmpty(response.data.task_id))
                    throw new PipelineApiException("Tripo returned no task ID for " + route + ".");
                return response.data.task_id;
            }
        }

        public async Task<TripoData> WaitForTaskAsync(
            string taskId,
            Action<float, string> progress,
            CancellationToken cancellationToken,
            TimeSpan? timeout = null)
        {
            DateTime deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromMinutes(25));
            int transientFailures = 0;

            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (var request = UnityWebRequest.Get(BaseUrl + "/tasks/" + UnityWebRequest.EscapeURL(taskId)))
                    {
                        request.SetRequestHeader("Authorization", "Bearer " + _apiKey);
                        request.timeout = 60;
                        await UnityWebRequestAsync.SendAsync(request, cancellationToken);
                        UnityWebRequestAsync.EnsureSuccess(request);
                        TripoResponse response = Parse(request.downloadHandler.text);
                        TripoData data = response.data;
                        if (data == null)
                            throw new PipelineApiException("Tripo task response has no data.");

                        string status = (data.status ?? string.Empty).ToLowerInvariant();
                        progress?.Invoke(Mathf.Clamp01(data.progress / 100f), status);
                        if (status == "success")
                            return data;
                        if (status == "failed" || status == "cancelled")
                            throw new PipelineApiException("Tripo task " + taskId + " ended with status: " + status + ".");
                        transientFailures = 0;
                    }
                }
                catch (PipelineApiException exception)
                {
                    bool transient = exception.StatusCode == 429 || exception.StatusCode >= 500 || exception.StatusCode == 0;
                    if (!transient || ++transientFailures > 5)
                        throw;
                }

                int seconds = Math.Min(15, 3 + transientFailures * 2);
                await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
            }

            throw new TimeoutException("Timed out waiting for Tripo task " + taskId + ".");
        }

        public async Task<byte[]> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new PipelineApiException("Tripo returned an invalid model URL.");

            for (int attempt = 1; attempt <= 4; attempt++)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(uri.AbsoluteUri))
                {
                    request.timeout = 180;
                    await UnityWebRequestAsync.SendAsync(request, cancellationToken);
                    if (request.result == UnityWebRequest.Result.Success && request.responseCode >= 200 && request.responseCode < 300)
                        return request.downloadHandler.data;
                    if (attempt == 4 || (request.responseCode != 429 && request.responseCode < 500))
                        UnityWebRequestAsync.EnsureSuccess(request);
                }
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
            }

            throw new PipelineApiException("Model download failed.");
        }

        private static TripoResponse Parse(string json)
        {
            TripoResponse response = JsonUtility.FromJson<TripoResponse>(json);
            if (response == null)
                throw new PipelineApiException("Tripo returned invalid JSON.");
            if (response.code != 0)
                throw new PipelineApiException("Tripo error " + response.code + ": " + response.message);
            return response;
        }
    }
}
