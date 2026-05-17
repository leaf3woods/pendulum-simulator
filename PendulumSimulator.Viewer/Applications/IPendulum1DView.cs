using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Viewer.Applications
{
    /// <summary>
    /// 渲染一维摆的观测。实现应在入口处验证
    /// <c>observation.Dimension == 1</c>。
    /// </summary>
    public interface IPendulum1DView
    {
        void Run(PendulumSystemField field, ThetaObservation observation);
    }
}
