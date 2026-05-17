namespace PendulumSimulator.Viewer.Rendering
{
    /// <summary>
    /// 每个具体视图通用的渲染配置：样本着色方式以及每个可视帧推进仿真的步数。
    /// </summary>
    public record RenderOptions
    {
        public PendulumColorScheme ColorScheme { get; init; } = PendulumColorScheme.RgbAngles;

        public double TimeStep { get; init; } = 0.02;

        public int SimulationStepsPerFrame { get; init; } = 2;
    }
}
