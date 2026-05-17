using PendulumSimulator.Analysis;
using Xunit;

namespace PendulumSimulator.Tests.Analysis
{
    public class PendulumSystemSpecTests
    {
        [Fact]
        public void UniformBuildsZeroInitialState()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 4);

            Assert.Equal(4, spec.PendulumCount);
            Assert.Equal(4, spec.DefaultThetas.Count);
            Assert.Equal(4, spec.DefaultOmegas.Count);
            Assert.All(spec.DefaultThetas, value => Assert.Equal(0.0, value));
            Assert.All(spec.DefaultOmegas, value => Assert.Equal(0.0, value));
        }

        [Fact]
        public void ValidateRejectsNonPositivePendulumCount()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 0,
                Mass = 1,
                Length = 1,
                DefaultThetas = [],
                DefaultOmegas = [],
            };

            Assert.Throws<ArgumentOutOfRangeException>(spec.Validate);
        }

        [Fact]
        public void ValidateRejectsNonPositiveMass()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 2,
                Mass = 0,
                Length = 1,
                DefaultThetas = [0, 0],
                DefaultOmegas = [0, 0],
            };

            Assert.Throws<ArgumentOutOfRangeException>(spec.Validate);
        }

        [Fact]
        public void ValidateRejectsNonPositiveLength()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 2,
                Mass = 1,
                Length = -0.1,
                DefaultThetas = [0, 0],
                DefaultOmegas = [0, 0],
            };

            Assert.Throws<ArgumentOutOfRangeException>(spec.Validate);
        }

        [Fact]
        public void ValidateRejectsMismatchedDefaultThetaLength()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 3,
                Mass = 1,
                Length = 1,
                DefaultThetas = [0, 0],
                DefaultOmegas = [0, 0, 0],
            };

            Assert.Throws<ArgumentException>(spec.Validate);
        }

        [Fact]
        public void ValidateRejectsMismatchedDefaultOmegaLength()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 3,
                Mass = 1,
                Length = 1,
                DefaultThetas = [0, 0, 0],
                DefaultOmegas = [0, 0],
            };

            Assert.Throws<ArgumentException>(spec.Validate);
        }
    }
}
