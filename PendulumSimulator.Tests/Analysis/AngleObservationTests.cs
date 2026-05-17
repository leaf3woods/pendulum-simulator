using PendulumSimulator.Analysis.Observation;
using Xunit;

namespace PendulumSimulator.Tests.Analysis
{
    public class AngleObservationTests
    {
        [Fact]
        public void SampleCountIsResolutionPowerDimension()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 1,
                Dimension = 3,
                Resolution = 4,
                Minimum = -Math.PI,
                Maximum = Math.PI,
            };

            Assert.Equal(64, observation.SampleCount);
        }

        [Fact]
        public void GetCoordinatesReturnsMixedRadixCoordinates()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 3,
                Resolution = 4,
                Minimum = -Math.PI,
                Maximum = Math.PI,
            };

            Assert.Equal([0, 0, 0], observation.GetCoordinates(0));
            Assert.Equal([1, 1, 0], observation.GetCoordinates(5));
            Assert.Equal([3, 3, 3], observation.GetCoordinates(63));
        }

        [Fact]
        public void MapThetaSpansRangeInclusive()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 1,
                Resolution = 3,
                Minimum = -1.0,
                Maximum = 1.0,
            };

            Assert.Equal(-1.0, observation.MapTarget(0), precision: 12);
            Assert.Equal(0.0, observation.MapTarget(1), precision: 12);
            Assert.Equal(1.0, observation.MapTarget(2), precision: 12);
        }

        [Fact]
        public void MapThetaSingletonReturnsRangeMidpoint()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 1,
                Resolution = 1,
                Minimum = -1.0,
                Maximum = 3.0,
            };

            Assert.Equal(1.0, observation.MapTarget(0), precision: 12);
        }

        [Fact]
        public void MapOmegaSpansRangeInclusive()
        {
            var observation = new OmegaObservation
            {
                StartIndex = 0,
                Dimension = 1,
                Resolution = 3,
                Minimum = -2.0,
                Maximum = 2.0,
            };

            Assert.Equal(-2.0, observation.MapTarget(0), precision: 12);
            Assert.Equal(0.0, observation.MapTarget(1), precision: 12);
            Assert.Equal(2.0, observation.MapTarget(2), precision: 12);
        }

        [Fact]
        public void ValidateRejectsNegativeStartPendulumIndex()
        {
            var observation = new ThetaObservation
            {
                StartIndex = -1,
                Dimension = 2,
                Resolution = 4,
                Minimum = -1,
                Maximum = 1,
            };

            Assert.Throws<ArgumentOutOfRangeException>(observation.Validate);
        }

        [Fact]
        public void ValidateRejectsNonPositiveDimension()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 0,
                Resolution = 4,
                Minimum = -1,
                Maximum = 1,
            };

            Assert.Throws<ArgumentOutOfRangeException>(observation.Validate);
        }

        [Fact]
        public void ValidateRejectsNonPositiveResolution()
        {
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 2,
                Resolution = 0,
                Minimum = -1,
                Maximum = 1,
            };

            Assert.Throws<ArgumentOutOfRangeException>(observation.Validate);
        }
    }
}
