namespace PendulumSimulator.Analysis.Observation
{
    /// <summary>
    /// 指定如何扫描一段连续的摆的初始角度，与具体的 N 摆系统无关。
    /// </summary>
    /// <remarks>
    /// 该观测与维度无关：<see cref="Dimension"/> 可为 1、2、3 或更高（用于纯分析）。
    /// 视图绑定会在其边界处限制维度。所有被观测的轴共享相同的范围和分辨率；样本构成一个均匀的
    /// <c>Resolution^Dimension</c> 网格。
    /// </remarks>
    public sealed record ThetaObservation : BaseObservation<double>
    {
        public static ThetaObservation Default { get; } = new();

        public override double Maximum { get; init; } = Math.PI;

        public override double Minimum { get; init; } = -Math.PI;

        /// <summary>
        /// 将轴坐标（在 [0, Resolution-1]）映射为对应的采样角值。
        /// </summary>
        public override double MapTarget(int coordinate)
        {
            if (Resolution <= 1)
                return (Minimum + Maximum) / 2.0;

            return Minimum
                + (double)coordinate / (Resolution - 1)
                * (Maximum - Minimum);
        }
    }
}
