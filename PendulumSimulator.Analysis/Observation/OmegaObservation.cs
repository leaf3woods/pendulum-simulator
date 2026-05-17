
namespace PendulumSimulator.Analysis.Observation
{
    public record class OmegaObservation : BaseObservation<double>
    {
        public static OmegaObservation Default { get; } = new();

        public override double Maximum { get; init; } = 20;

        public override double Minimum { get; init; } = 0;

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
