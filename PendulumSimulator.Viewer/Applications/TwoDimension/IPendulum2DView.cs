using PendulumSimulator.Analysis;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Viewer.Applications.TwoDimension
{
    /// <summary>
    /// 渲染二维摆的观测。实现应在入口处验证
    /// <c>observation.Dimension == 2</c>。
    /// </summary>
    public interface IPendulum2DView
    {
        void Run(PendulumSystemField field, ThetaObservation observation);
    }
}
