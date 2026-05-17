namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// 指定如何扫描一段连续的摆的初始角度，与具体的 N 摆系统无关。
    /// </summary>
    /// <remarks>
    /// 该观测与维度无关：<see cref="Dimension"/> 可为 1、2、3 或更高（用于纯分析）。
    /// 视图绑定会在其边界处限制维度。所有被观测的轴共享相同的范围和分辨率；样本构成一个均匀的
    /// <c>Resolution^Dimension</c> 网格。
    /// </remarks>
    public sealed record ThetaObservation
    {
        public static ThetaObservation Default { get; } = new();

        public int StartPendulumIndex { get; init; } = 0;

        public int Dimension { get; init; } = 2;

        public int Resolution { get; init; } = 256;

        public double ThetaMin { get; init; } = -Math.PI;

        public double ThetaMax { get; init; } = Math.PI;

        public int SampleCount => checked((int)Math.Pow(Resolution, Dimension));

        public void Validate()
        {
            if (StartPendulumIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(StartPendulumIndex), "Start pendulum index cannot be negative.");
            if (Dimension <= 0)
                throw new ArgumentOutOfRangeException(nameof(Dimension), "Dimension must be greater than 0.");
            if (Resolution <= 0)
                throw new ArgumentOutOfRangeException(nameof(Resolution), "Resolution must be greater than 0.");
        }

        /// <summary>
        /// 将线性样本索引解码为对被观测轴的混基坐标。
        /// </summary>
        /// <remarks>
        /// 布局为列主序：<c>i = c[0] + c[1] * R + c[2] * R^2 + ...</c>。
        /// </remarks>
        public int[] GetCoordinates(int sampleIndex)
        {
            if (sampleIndex < 0 || sampleIndex >= SampleCount)
                throw new ArgumentOutOfRangeException(nameof(sampleIndex));

            int[] coordinates = new int[Dimension];
            int remaining = sampleIndex;

            for (int axis = 0; axis < Dimension; axis++)
            {
                coordinates[axis] = remaining % Resolution;
                remaining /= Resolution;
            }

            return coordinates;
        }

        /// <summary>
        /// 将轴坐标（在 [0, Resolution-1]）映射为对应的采样角值。
        /// </summary>
        public double MapTheta(int coordinate)
        {
            if (Resolution <= 1)
                return (ThetaMin + ThetaMax) / 2.0;

            return ThetaMin
                + (double)coordinate / (Resolution - 1)
                * (ThetaMax - ThetaMin);
        }
    }
}
