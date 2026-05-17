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
            var field = new PendulumSystemField(
            [
                CreateSystem(0.1, 0.2),
                CreateSystem(0.3, 0.4)
            ]);
            double[][] before = field.Systems.Select(system => system.ToStateVector()).ToArray();

            field.Step(dt: 0.01, steps: 10);

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

        static PendulumSystem CreateSystem(double theta0, double theta1)
        {
            return new PendulumSystem(
            [
                new Pendulum(theta: theta0),
                new Pendulum(theta: theta1)
            ]);
        }
    }
}
