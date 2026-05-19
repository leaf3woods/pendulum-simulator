using PendulumSimulator.Core.PhysicsSystem;
using Xunit;

namespace PendulumSimulator.Tests.Core.PhysicsSystem
{
    public class PendulumSystemFieldTests
    {
        [Fact]
        public void HoldsSystemsWithTheSamePendulumCount()
        {
            var field = new PendulumSystemField(
            [
                CreateSystem(0.1, 0.2),
                CreateSystem(0.3, 0.4)
            ]);

            Assert.Equal(2, field.Count);
            Assert.Equal(2, field.PendulumCount);
            Assert.Same(field.Systems[0], field[0]);
        }

        [Fact]
        public void StepAdvancesEverySystem()
        {
            var field = CreateField();
            double[][] before = field.Systems.Select(system => system.ToStateVector()).ToArray();

            field.Step(dt: 0.01, steps: 10);

            AssertEverySystemAdvanced(field, before);
        }

        [Fact]
        public void GpuStepAdvancesEverySystem()
        {
            var field = CreateField(pendulumCount: 2);
            double[][] before = field.Systems.Select(system => system.ToStateVector()).ToArray();

            field.Step(dt: 0.01, steps: 10, useGpu: true);

            AssertEverySystemAdvanced(field, before);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void GpuStepAdvancesEverySystemForLargerSupportedPendulumCounts(int pendulumCount)
        {
            var field = CreateField(pendulumCount);
            double[][] before = field.Systems.Select(system => system.ToStateVector()).ToArray();

            field.Step(dt: 0.01, steps: 10, useGpu: true);

            AssertEverySystemAdvanced(field, before);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GpuStepStaysCloseToCpuForSupportedPendulumCounts(int pendulumCount)
        {
            var cpuField = CreateField(pendulumCount);
            var gpuField = CreateField(pendulumCount);

            cpuField.Step(dt: 0.002, steps: 5, useGpu: false);
            gpuField.Step(dt: 0.002, steps: 5, useGpu: true);

            for (int systemIndex = 0; systemIndex < cpuField.Count; systemIndex++)
            {
                double[] cpuState = cpuField[systemIndex].ToStateVector();
                double[] gpuState = gpuField[systemIndex].ToStateVector();

                for (int stateIndex = 0; stateIndex < cpuState.Length; stateIndex++)
                {
                    Assert.True(
                        Math.Abs(cpuState[stateIndex] - gpuState[stateIndex]) < 1e-3,
                        $"State mismatch for {pendulumCount} pendulums at system {systemIndex}, state {stateIndex}: CPU={cpuState[stateIndex]}, GPU={gpuState[stateIndex]}.");
                }
            }
        }

        static PendulumSystemField CreateField(int pendulumCount = 2)
        {
            return new PendulumSystemField(
            [
                CreateSystem(BuildThetas(pendulumCount, 0.1)),
                CreateSystem(BuildThetas(pendulumCount, 0.3)),
                CreateSystem(BuildThetas(pendulumCount, 0.5))
            ]);
        }

        static void AssertEverySystemAdvanced(PendulumSystemField field, double[][] before)
        {
            double[][] after = field.Systems.Select(system => system.ToStateVector()).ToArray();
            for (int i = 0; i < before.Length; i++)
            {
                Assert.NotEqual(before[i], after[i]);
                Assert.All(after[i], value =>
                    Assert.True(double.IsFinite(value), $"Expected finite value, actual {value}."));
            }
        }

        [Fact]
        public void ConstructorRejectsMixedPendulumCounts()
        {
            Assert.Throws<ArgumentException>(() =>
                new PendulumSystemField(
                [
                    CreateSystem(0.1, 0.2),
                    new PendulumSystem(
                    [
                        new Pendulum(theta: 0.1)
                    ])
                ]));
        }

        static double[] BuildThetas(int pendulumCount, double start)
        {
            var thetas = new double[pendulumCount];

            for (int i = 0; i < pendulumCount; i++)
            {
                thetas[i] = start + i * 0.1;
            }

            return thetas;
        }

        static PendulumSystem CreateSystem(params double[] thetas)
        {
            return new PendulumSystem(thetas.Select(theta => new Pendulum(theta: theta)));
        }
    }
}
