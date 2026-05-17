using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    public sealed class ThetaObservedField : IObservedField
    {
        private readonly int[][] _coordinates;
        private readonly double[][] _initialThetas;

        public ThetaObservedField(PendulumSystemField field, ThetaObservation observation)
        {
            ArgumentNullException.ThrowIfNull(field);
            ArgumentNullException.ThrowIfNull(observation);

            observation.Validate();
            ValidateCompatibility(field, observation);

            Field = field;
            Observation = observation;
            _coordinates = new int[field.Count][];
            _initialThetas = new double[field.Count][];

            for (int sampleIndex = 0; sampleIndex < field.Count; sampleIndex++)
            {
                _coordinates[sampleIndex] = observation.GetCoordinates(sampleIndex);
                _initialThetas[sampleIndex] = field[sampleIndex]
                    .Pendulums
                    .Select(pendulum => pendulum.Theta)
                    .ToArray();
            }
        }

        public PendulumSystemField Field { get; }

        public ThetaObservation Observation { get; }

        IObservation IObservedField.Observation => Observation;

        public int Count => Field.Count;

        public int PendulumCount => Field.PendulumCount;

        public PendulumSystem this[int sampleIndex] => Field[sampleIndex];

        public IReadOnlyList<int> GetCoordinates(int sampleIndex)
        {
            ValidateSampleIndex(sampleIndex);

            return (int[])_coordinates[sampleIndex].Clone();
        }

        public ObservedSystemSample GetSample(int sampleIndex)
        {
            ValidateSampleIndex(sampleIndex);

            return new ObservedSystemSample(
                sampleIndex,
                (int[])_coordinates[sampleIndex].Clone(),
                (double[])_initialThetas[sampleIndex].Clone(),
                Field[sampleIndex]);
        }

        static void ValidateCompatibility(PendulumSystemField field, ThetaObservation observation)
        {
            if (field.Count != observation.SampleCount)
                throw new ArgumentException(
                    $"Field sample count ({field.Count}) must equal observation sample count ({observation.SampleCount}).",
                    nameof(field));

            if (observation.StartIndex + observation.Dimension > field.PendulumCount)
                throw new ArgumentException(
                    $"Observed angle range [{observation.StartIndex}, {observation.StartIndex + observation.Dimension}) "
                    + $"exceeds pendulum count ({field.PendulumCount}).",
                    nameof(observation));
        }

        void ValidateSampleIndex(int sampleIndex)
        {
            if (sampleIndex < 0 || sampleIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(sampleIndex));
        }
    }
}
