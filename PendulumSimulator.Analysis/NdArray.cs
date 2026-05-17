namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// 列主序线性布局的密集 N 维数组，用于存放由 <see cref="ThetaObservation"/> 索引的每个样本的分析结果。
    /// </summary>
    /// <remarks>
    /// 占位类型。后续的分析器（混沌/稳定性指标等）将产生 <c>NdArray&lt;double&gt;</c> 或类似类型，视图可直接消费。
    /// </remarks>
    public sealed class NdArray<T>
    {
        private readonly T[] _values;
        private readonly int[] _strides;

        public NdArray(IReadOnlyList<int> shape)
        {
            ArgumentNullException.ThrowIfNull(shape);
            if (shape.Count == 0)
                throw new ArgumentException("Shape must have at least one dimension.", nameof(shape));

            Shape = shape.ToArray();
            _strides = new int[shape.Count];
            int total = 1;

            for (int i = 0; i < shape.Count; i++)
            {
                if (shape[i] <= 0)
                    throw new ArgumentException($"Shape[{i}] must be positive.", nameof(shape));

                _strides[i] = total;
                total = checked(total * shape[i]);
            }

            _values = new T[total];
        }

        public IReadOnlyList<int> Shape { get; }

        public int Length => _values.Length;

        public T this[params int[] coordinates]
        {
            get => _values[ToLinearIndex(coordinates)];
            set => _values[ToLinearIndex(coordinates)] = value;
        }

        public T GetAt(int linearIndex) => _values[linearIndex];

        public void SetAt(int linearIndex, T value) => _values[linearIndex] = value;

        int ToLinearIndex(int[] coordinates)
        {
            if (coordinates.Length != Shape.Count)
                throw new ArgumentException(
                    $"Coordinate count ({coordinates.Length}) must equal rank ({Shape.Count}).",
                    nameof(coordinates));

            int index = 0;
            for (int axis = 0; axis < coordinates.Length; axis++)
            {
                int coordinate = coordinates[axis];
                if (coordinate < 0 || coordinate >= Shape[axis])
                    throw new ArgumentOutOfRangeException(nameof(coordinates));

                index += coordinate * _strides[axis];
            }

            return index;
        }
    }
}
