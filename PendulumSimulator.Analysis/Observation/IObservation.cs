
namespace PendulumSimulator.Analysis.Observation
{
    public interface IObservation<TTarget> : IObservation where TTarget : struct
    {

        public TTarget Maximum { get; }

        public TTarget Minimum { get; }

        public TTarget MapTarget(int coordinate);
    }

    public interface IObservation 
    {
        public int Dimension { get; }

        public int Resolution { get; }

        public int SampleCount { get; }

        public int StartIndex { get; }

        public int[] GetCoordinates(int sampleIndex);
        public void Validate();
    }
}
