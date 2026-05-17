using PendulumSimulator.Analysis;
using PendulumSimulator.Core.PhysicsSystem;
using Xunit;

namespace PendulumSimulator.Tests.Analysis
{
    public class PendulumSystemFieldFactoryTests
    {
        [Fact]
        public void BuildsResolutionPowerDimensionSystems()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 5);
            var observation = new ThetaObservation
            {
                StartPendulumIndex = 1,
                Dimension = 3,
                Resolution = 4,
                ThetaMin = -Math.PI,
                ThetaMax = Math.PI,
            };

            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, observation);

            Assert.Equal(64, field.Count);
            Assert.Equal(5, field.PendulumCount);
        }

        [Fact]
        public void MapsContinuousObservedAngles()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 5,
                Mass = 1,
                Length = 1,
                DefaultThetas = [0.25, 0.25, 0.25, 0.25, 0.25],
                DefaultOmegas = [0, 0, 0, 0, 0],
            };
            var observation = new ThetaObservation
            {
                StartPendulumIndex = 1,
                Dimension = 3,
                Resolution = 3,
                ThetaMin = -1.0,
                ThetaMax = 1.0,
            };

            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, observation);

            PendulumSystem first = field[0];
            PendulumSystem last = field[field.Count - 1];

            Assert.Equal(0.25, first[0].Theta, precision: 12);
            Assert.Equal(-1.0, first[1].Theta, precision: 12);
            Assert.Equal(-1.0, first[2].Theta, precision: 12);
            Assert.Equal(-1.0, first[3].Theta, precision: 12);
            Assert.Equal(0.25, first[4].Theta, precision: 12);

            Assert.Equal(1.0, last[1].Theta, precision: 12);
            Assert.Equal(1.0, last[2].Theta, precision: 12);
            Assert.Equal(1.0, last[3].Theta, precision: 12);
        }

        [Fact]
        public void InitialAngularVelocityFollowsSpecDefaults()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 3);
            var observation = new ThetaObservation
            {
                StartPendulumIndex = 0,
                Dimension = 2,
                Resolution = 3,
                ThetaMin = -1,
                ThetaMax = 1,
            };

            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, observation);

            foreach (PendulumSystem system in field.Systems)
            {
                Assert.All(system.Pendulums, pendulum => Assert.Equal(0.0, pendulum.Omega, precision: 12));
            }
        }

        [Fact]
        public void BuildRejectsObservedRangeOutsidePendulumCount()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 3);
            var observation = new ThetaObservation
            {
                StartPendulumIndex = 1,
                Dimension = 3,
                Resolution = 4,
                ThetaMin = -1,
                ThetaMax = 1,
            };

            Assert.Throws<ArgumentException>(() => PendulumSystemFieldFactory.Build(spec, observation));
        }
    }
}
