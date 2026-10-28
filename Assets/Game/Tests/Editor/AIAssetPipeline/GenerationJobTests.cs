using NUnit.Framework;
using UnityEngine;

namespace Game.Editor.AIAssetPipeline.Tests
{
    public sealed class GenerationJobTests
    {
        [Test]
        public void EmptyAnimationsProduceStaticJob()
        {
            GenerationJob job = GenerationJob.Create("Elf", "An elf", GeneratedAssetKind.Human, string.Empty);

            Assert.IsFalse(job.HasRequestedAnimations);
            Assert.IsFalse(job.RequiresMesh2Motion);
        }

        [Test]
        public void HumansAndAnimalsWithAnimationsRequireMesh2Motion()
        {
            GenerationJob human = GenerationJob.Create("Elf", "An elf", GeneratedAssetKind.Human, "Idle\nWalk");
            GenerationJob animal = GenerationJob.Create("Wolf", "A wolf", GeneratedAssetKind.Animal, "Idle, Run");

            Assert.IsTrue(human.RequiresMesh2Motion);
            Assert.IsTrue(animal.RequiresMesh2Motion);
            CollectionAssert.AreEqual(new[] { "Idle", "Walk" }, human.requestedAnimations);
            CollectionAssert.AreEqual(new[] { "Idle", "Run" }, animal.requestedAnimations);
        }

        [Test]
        public void JobRoundTripsThroughJson()
        {
            GenerationJob original = GenerationJob.Create("Forest Elf", "An elf", GeneratedAssetKind.Human, "Idle;Walk");
            original.stage = GenerationStage.GeneratingModel;
            original.tripoModelTaskId = "task_example";

            string json = JsonUtility.ToJson(original);
            GenerationJob restored = JsonUtility.FromJson<GenerationJob>(json);

            Assert.AreEqual(original.slug, restored.slug);
            Assert.AreEqual(GenerationStage.GeneratingModel, restored.stage);
            Assert.AreEqual("task_example", restored.tripoModelTaskId);
            CollectionAssert.AreEqual(original.requestedAnimations, restored.requestedAnimations);
        }
    }
}
