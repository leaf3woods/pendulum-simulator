using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// 通过将系统蓝图 (<see cref="PendulumSystemSpec"/>) 与观测扫描 (<see cref="ThetaObservation"/>)
    /// 组合来构建 <see cref="PendulumSystemField"/>。
    /// </summary>
    public static class PendulumSystemFieldFactory
    {
        public static PendulumSystemField Build<TTarget>(PendulumSystemSpec spec, IObservation<TTarget> observation)
            where TTarget : struct
        {
            ArgumentNullException.ThrowIfNull(spec);
            ArgumentNullException.ThrowIfNull(observation);

            spec.Validate();
            observation.Validate();
            ValidateCompatibility(spec, observation);

            var sampleCount = observation.SampleCount;
            var systems = new PendulumSystem[sampleCount];

            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                systems[sampleIndex] = BuildSystemAt(spec, observation, sampleIndex);
            }

            return new PendulumSystemField(systems);
        }

        static void ValidateCompatibility(PendulumSystemSpec spec, IObservation observation)
        {
            if (observation.StartIndex + observation.Dimension > spec.PendulumCount)
                throw new ArgumentException(
                    $"Observed angle range [{observation.StartIndex}, {observation.StartIndex + observation.Dimension}) "
                    + $"exceeds pendulum count ({spec.PendulumCount}).",
                    nameof(observation));
        }

        static PendulumSystem BuildSystemAt<TTarget>(PendulumSystemSpec spec,
            IObservation<TTarget> observation, int sampleIndex)
            where TTarget : struct
        {
            // 将线性样本索引还原为观测空间坐标，再映射到对应摆的初始角度。
            int[] coordinates = observation.GetCoordinates(sampleIndex);
            var pendulums = new Pendulum[spec.PendulumCount];

            for (var i = 0; i < spec.PendulumCount; i++)
            {
                pendulums[i] = new Pendulum(
                    theta: spec.DefaultThetas[i],
                    omega: spec.DefaultOmegas[i],
                    mass: spec.Mass,
                    length: spec.Length);
            }

            for (var axis = 0; axis < observation.Dimension; axis++)
            {
                var pendulumIndex = observation.StartIndex + axis;
                if(observation is ThetaObservation)
                {
                    pendulums[pendulumIndex].Theta = Convert.ToDouble(observation.MapTarget(coordinates[axis]));
                }
                else if (observation is OmegaObservation)
                {
                    pendulums[pendulumIndex].Omega = Convert.ToDouble(observation.MapTarget(coordinates[axis]));
                }
                else
                {
                    throw new NotSupportedException($"Unsupported observation target type: {typeof(TTarget)}.");
                }

            }

            return new PendulumSystem(pendulums);
        }
    }
}
