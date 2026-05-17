using PendulumSimulator.Core.PhysicsSystem;
using Xunit;

namespace PendulumSimulator.Tests.Core.PhysicsSystem
{
    public class PendulumSystemTests
    {
        [Fact]
        public void SupportsMoreThanTwoPendulums()
        {
            var system = new PendulumSystem(
            [
                new Pendulum(theta: 0.3),
                new Pendulum(theta: 0.2),
                new Pendulum(theta: 0.1),
                new Pendulum(theta: -0.1)
            ]);

            Assert.Equal(4, system.Count);
            Assert.Equal(8, system.ToStateVector().Length);
        }

        [Fact]
        public void ThreePendulumSystemRemainsFiniteAfterManySteps()
        {
            var system = new PendulumSystem(
            [
                new Pendulum(theta: 0.9, omega: 0.1),
                new Pendulum(theta: 0.4, omega: -0.2),
                new Pendulum(theta: -0.2, omega: 0.3)
            ]);

            for (int i = 0; i < 1000; i++)
            {
                system.Step(0.002);
            }

            Assert.All(system.ToStateVector(), value =>
                Assert.True(double.IsFinite(value), $"Expected finite value, actual {value}."));
        }
    }
}
