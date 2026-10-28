using NUnit.Framework;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline.Tests
{
    public sealed class ApiContractTests
    {
        [Test]
        public void ParsesTripoTaskFixture()
        {
            const string json = "{\"code\":0,\"data\":{\"task_id\":\"task_123\",\"status\":\"success\",\"progress\":100,\"credits_consumed\":42.5,\"output\":{\"model_url\":\"https://example.test/model.glb\",\"riggable\":true,\"rig_type\":\"biped\"}}}";

            TripoResponse response = JsonUtility.FromJson<TripoResponse>(json);

            Assert.AreEqual(0, response.code);
            Assert.AreEqual("task_123", response.data.task_id);
            Assert.AreEqual("https://example.test/model.glb", response.data.output.model_url);
            Assert.IsTrue(response.data.output.riggable);
            Assert.AreEqual(42.5f, response.data.credits_consumed);
        }

        [Test]
        public void ParsesOpenAiImageFixture()
        {
            const string json = "{\"data\":[{\"b64_json\":\"aGVsbG8=\",\"revised_prompt\":\"test\"}]}";

            OpenAIImageResponse response = JsonUtility.FromJson<OpenAIImageResponse>(json);

            Assert.AreEqual("aGVsbG8=", response.data[0].b64_json);
        }
    }
}
