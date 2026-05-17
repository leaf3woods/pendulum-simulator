using PendulumSimulator.Analysis;
using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;
using Xunit;

namespace PendulumSimulator.Tests.Analysis
{
    public class ThetaObservedFieldTests
    {
        [Fact]
        public void GetSampleReturnsCoordinatesAndSystem()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 5);
            var observation = new ThetaObservation
            {
                StartIndex = 1,
                Dimension = 3,
                Resolution = 4,
                Minimum = -1.0,
                Maximum = 1.0,
            };
            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, observation);
            var observedField = new ThetaObservedField(field, observation);

            ObservedSystemSample sample = observedField.GetSample(5);

            Assert.Equal(5, sample.SampleIndex);
            Assert.Equal([1, 1, 0], sample.Coordinates);
            Assert.Same(field[5], sample.System);
            Assert.Equal(field.PendulumCount, sample.InitialThetas.Count);
        }

        [Fact]
        public void GetSampleSupportsHigherDimensionalCoordinates()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 4);
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 4,
                Resolution = 3,
                Minimum = -1.0,
                Maximum = 1.0,
            };

            ThetaObservedField observedField = PendulumSystemFieldFactory.BuildObserved(spec, observation);

            Assert.Equal([2, 1, 2, 0], observedField.GetSample(23).Coordinates);
        }

        [Fact]
        public void InitialThetasStayBoundToOriginalFieldCoordinates()
        {
            var spec = new PendulumSystemSpec
            {
                PendulumCount = 2,
                Mass = 1,
                Length = 1,
                DefaultThetas = [0.25, 0.75],
                DefaultOmegas = [0.0, 0.0],
            };
            var observation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 1,
                Resolution = 3,
                Minimum = -1.0,
                Maximum = 1.0,
            };
            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, observation);
            var observedField = new ThetaObservedField(field, observation);

            field[2].ApplyStateVector([0.123, 0.456, 2.0, 3.0]);

            ObservedSystemSample sample = observedField.GetSample(2);

            Assert.Equal([1.0, 0.75], sample.InitialThetas);
            Assert.Equal(0.123, sample.System[0].Theta, precision: 12);
            Assert.Equal(0.456, sample.System[1].Theta, precision: 12);
        }

        [Fact]
        public void ConstructorRejectsMismatchedSampleCount()
        {
            PendulumSystemSpec spec = PendulumSystemSpec.Uniform(pendulumCount: 2);
            var fieldObservation = new ThetaObservation
            {
                StartIndex = 0,
                Dimension = 2,
                Resolution = 2,
            };
            var mismatchedObservation = fieldObservation with { Resolution = 3 };
            PendulumSystemField field = PendulumSystemFieldFactory.Build(spec, fieldObservation);

            Assert.Throws<ArgumentException>(() => new ThetaObservedField(field, mismatchedObservation));
        }
    }
}
