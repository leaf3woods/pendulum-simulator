using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    public sealed record ObservedSystemSample(
        int SampleIndex,
        IReadOnlyList<int> Coordinates,
        IReadOnlyList<double> InitialThetas,
        PendulumSystem System);
}
