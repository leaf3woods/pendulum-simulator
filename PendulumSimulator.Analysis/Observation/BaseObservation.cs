
namespace PendulumSimulator.Analysis.Observation
{
    public abstract record class BaseObservation<TTarget> : IObservation<TTarget> where TTarget : struct
    {
        public int Dimension { get; init; } = 2;

        public int Resolution { get; init; } = 256;

        public int StartIndex { get; init; } = 0;

        public int SampleCount => checked((int)Math.Pow(Resolution, Dimension));

        public virtual TTarget Maximum { get; init; }

        public virtual TTarget Minimum { get; init; }

        /// <summary>
        /// 将线性样本索引解码为对被观测轴的混基坐标。
        /// </summary>
        /// <remarks>
        /// 布局为列主序：<c>i = c[0] + c[1] * R + c[2] * R^2 + ...</c>。
        /// </remarks>
        public virtual int[] GetCoordinates(int sampleIndex)
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

        public virtual void Validate()
        {
            if (StartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(StartIndex), "Start pendulum index cannot be negative.");
            if (Dimension <= 0)
                throw new ArgumentOutOfRangeException(nameof(Dimension), "Dimension must be greater than 0.");
            if (Resolution <= 0)
                throw new ArgumentOutOfRangeException(nameof(Resolution), "Resolution must be greater than 0.");
        }

        public abstract TTarget MapTarget(int coordinate);
    }
}
