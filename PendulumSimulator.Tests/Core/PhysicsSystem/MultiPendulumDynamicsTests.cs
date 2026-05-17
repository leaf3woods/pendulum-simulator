using PendulumSimulator.Core;
using PendulumSimulator.Core.PhysicsSystem;
using Xunit;

namespace PendulumSimulator.Tests.Core.PhysicsSystem
{
    public class MultiPendulumDynamicsTests
    {
        [Fact]
        public void SinglePendulumDerivativeMatchesAnalyticFormula()
        {
            var system = new PendulumSystem(
            [
                new Pendulum(theta: 0.5, omega: 0.25, mass: 2.0, length: 1.5)
            ]);
            var dynamics = new MultiPendulumDynamics();

            double[] derivative = dynamics.Derivative(system, system.ToStateVector());

            Assert.Equal(0.25, derivative[0], precision: 12);
            AssertClose(-(Physics.G / 1.5) * Math.Sin(0.5), derivative[1], tolerance: 1e-12);
        }

        [Theory]
        [InlineData(0.5, 1.0, 0.0, 0.0)]
        [InlineData(1.2, -0.8, 0.4, -0.2)]
        [InlineData(-2.4, 2.1, 1.5, -1.1)]
        [InlineData(0.01, 0.02, -0.5, 0.7)]
        public void DoublePendulumDerivativeMatchesLegacyFormula(
            double theta1,
            double theta2,
            double omega1,
            double omega2)
        {
            var system = new PendulumSystem(
            [
                new Pendulum(theta: theta1, omega: omega1, mass: 1.0, length: 1.0),
                new Pendulum(theta: theta2, omega: omega2, mass: 1.0, length: 1.0)
            ]);
            var dynamics = new MultiPendulumDynamics();

            double[] derivative = dynamics.Derivative(system, system.ToStateVector());
            (double alpha1, double alpha2) = LegacyDoublePendulumAcceleration(
                theta1,
                theta2,
                omega1,
                omega2,
                m1: 1.0,
                m2: 1.0,
                l1: 1.0,
                l2: 1.0);

            Assert.Equal(omega1, derivative[0], precision: 12);
            Assert.Equal(omega2, derivative[1], precision: 12);
            AssertClose(alpha1, derivative[2], tolerance: 1e-9);
            AssertClose(alpha2, derivative[3], tolerance: 1e-9);
        }

        static (double alpha1, double alpha2) LegacyDoublePendulumAcceleration(
            double theta1,
            double theta2,
            double omega1,
            double omega2,
            double m1,
            double m2,
            double l1,
            double l2)
        {
            double delta = theta1 - theta2;
            double denominator = 2 * m1 + m2 - m2 * Math.Cos(2 * theta1 - 2 * theta2);

            double alpha1 =
                (
                    -Physics.G * (2 * m1 + m2) * Math.Sin(theta1)
                    - m2 * Physics.G * Math.Sin(theta1 - 2 * theta2)
                    - 2 * Math.Sin(delta) * m2 *
                    (
                        omega2 * omega2 * l2
                        + omega1 * omega1 * l1 * Math.Cos(delta)
                    )
                )
                /
                (l1 * denominator);

            double alpha2 =
                (
                    2 * Math.Sin(delta) *
                    (
                        omega1 * omega1 * l1 * (m1 + m2)
                        + Physics.G * (m1 + m2) * Math.Cos(theta1)
                        + omega2 * omega2 * l2 * m2 * Math.Cos(delta)
                    )
                )
                /
                (l2 * denominator);

            return (alpha1, alpha2);
        }

        static void AssertClose(double expected, double actual, double tolerance)
        {
            Assert.True(
                Math.Abs(expected - actual) <= tolerance,
                $"Expected {expected}, actual {actual}, tolerance {tolerance}.");
        }
    }
}
