using PendulumSimulator.Analysis;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Viewer.Applications.ThreeDimension
{
    /// <summary>
    /// 3D 观测构建器的占位实现。目前仅报告观测的形状；真正的体积渲染器尚未实现。
    /// </summary>
    public class Builder3DView : IPendulum3DView
    {
        private readonly Builder3DViewOptions _options;

        public Builder3DView(Builder3DViewOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
        }

        public void Run(PendulumSystemField field, ThetaObservation observation)
        {
            ArgumentNullException.ThrowIfNull(field);
            ArgumentNullException.ThrowIfNull(observation);

            if (observation.Dimension != 3)
                throw new ArgumentException(
                    $"Builder3DView requires a 3D observation; got Dimension={observation.Dimension}.",
                    nameof(observation));

            Console.WriteLine("3D observation built (stub renderer).");
            Console.WriteLine($"x=theta[{observation.StartPendulumIndex}], y=theta[{observation.StartPendulumIndex + 1}], z=theta[{observation.StartPendulumIndex + 2}]");
            Console.WriteLine($"resolution : {observation.Resolution}^3");
            Console.WriteLine($"systems    : {field.Count}");
            Console.WriteLine($"scheme     : {_options.Render.ColorScheme}");
        }
    }
}
