using NUnit.Framework;

namespace Game.Editor.AIAssetPipeline.Tests
{
    public sealed class PromptBuilderTests
    {
        [TestCase(GeneratedAssetKind.Human, "T-pose")]
        [TestCase(GeneratedAssetKind.Animal, "all of its paws")]
        [TestCase(GeneratedAssetKind.Prop, "canonical upright orientation")]
        public void BuildAddsKindSpecificPoseAndSharedTransparency(GeneratedAssetKind kind, string expectedPose)
        {
            string prompt = PromptBuilder.Build("A forest guardian", kind);

            StringAssert.Contains("A forest guardian", prompt);
            StringAssert.Contains(expectedPose, prompt);
            StringAssert.Contains("fully transparent background", prompt);
            StringAssert.Contains("low-poly", prompt);
        }

        [Test]
        public void BuildRejectsBlankDescription()
        {
            Assert.Throws<System.ArgumentException>(() => PromptBuilder.Build("  ", GeneratedAssetKind.Prop));
        }
    }
}
