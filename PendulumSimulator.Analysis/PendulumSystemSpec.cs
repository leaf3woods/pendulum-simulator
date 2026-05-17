namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// N 摆系统的蓝图，与观测或可视化方式无关。
    /// </summary>
    public sealed record PendulumSystemSpec
    {
        public static PendulumSystemSpec Default { get; } = new()
        {
            PendulumCount = 2,
            Mass = 1.0,
            Length = 1.0,
            DefaultThetas = [0.0, 0.0],
            DefaultOmegas = [0.0, 0.0],
        };

        public int PendulumCount { get; init; } = 2;

        public double Mass { get; init; } = 1.0;

        public double Length { get; init; } = 1.0;

        public IReadOnlyList<double> DefaultThetas { get; init; } = [];

        public IReadOnlyList<double> DefaultOmegas { get; init; } = [];

        public void Validate()
        {
            if (PendulumCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(PendulumCount), "Pendulum count must be greater than 0.");
            if (Mass <= 0)
                throw new ArgumentOutOfRangeException(nameof(Mass), "Mass must be greater than 0.");
            if (Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(Length), "Length must be greater than 0.");
            if (DefaultThetas.Count != PendulumCount)
                throw new ArgumentException(
                    $"DefaultThetas length ({DefaultThetas.Count}) must equal PendulumCount ({PendulumCount}).",
                    nameof(DefaultThetas));
            if (DefaultOmegas.Count != PendulumCount)
                throw new ArgumentException(
                    $"DefaultOmegas length ({DefaultOmegas.Count}) must equal PendulumCount ({PendulumCount}).",
                    nameof(DefaultOmegas));
        }

        public static PendulumSystemSpec Uniform(int pendulumCount, double mass = 1.0, double length = 1.0)
        {
            return new PendulumSystemSpec
            {
                PendulumCount = pendulumCount,
                Mass = mass,
                Length = length,
                DefaultThetas = new double[pendulumCount],
                DefaultOmegas = new double[pendulumCount],
            };
        }
    }
}
