using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    public interface IObservedField
    {
        PendulumSystemField Field { get; }

        IObservation Observation { get; }

        int Count { get; }

        int PendulumCount { get; }

        IReadOnlyList<int> GetCoordinates(int sampleIndex);

        ObservedSystemSample GetSample(int sampleIndex);
    }
}
