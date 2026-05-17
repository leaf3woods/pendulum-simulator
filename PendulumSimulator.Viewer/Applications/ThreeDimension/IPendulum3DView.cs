using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Viewer.Applications.ThreeDimension
{
    /// <summary>
    /// Renders a 3D pendulum observation. Implementations must verify
    /// <c>observation.Dimension == 3</c> on entry.
    /// </summary>
    public interface IPendulum3DView
    {
        void Run(PendulumSystemField field, ThetaObservation observation);
    }
}
