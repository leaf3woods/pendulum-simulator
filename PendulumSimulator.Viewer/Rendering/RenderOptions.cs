namespace PendulumSimulator.Viewer.Rendering
{
    /// <summary>
    /// 每个具体视图通用的渲染配置：样本着色方式以及每个可视帧推进仿真的步数。
    /// </summary>
    public record RenderOptions
    {
        public PendulumColorScheme ColorScheme { get; init; } = PendulumColorScheme.RgbAngles;

        /// <summary>
        /// 单次数值积分推进的物理时间长度。
        /// </summary>
        public double TimeStep { get; init; } = 0.02;

        /// <summary>
        /// 每个可视帧之前执行的积分步数；与 <see cref="TimeStep"/> 相乘得到每帧物理时间。
        /// </summary>
        public int SimulationStepsPerFrame { get; init; } = 2;
    }
}
