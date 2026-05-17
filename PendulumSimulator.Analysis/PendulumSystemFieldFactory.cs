using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// 通过将系统蓝图 (<see cref="PendulumSystemSpec"/>) 与观测扫描 (<see cref="ThetaObservation"/>)
    /// 组合来构建 <see cref="PendulumSystemField"/>。
    /// </summary>
    public static class PendulumSystemFieldFactory
    {
        public static PendulumSystemField Build(PendulumSystemSpec spec, ThetaObservation observation)
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

        static void ValidateCompatibility(PendulumSystemSpec spec, ThetaObservation observation)
        {
            if (observation.StartPendulumIndex + observation.Dimension > spec.PendulumCount)
                throw new ArgumentException(
                    $"Observed angle range [{observation.StartPendulumIndex}, {observation.StartPendulumIndex + observation.Dimension}) "
                    + $"exceeds pendulum count ({spec.PendulumCount}).",
                    nameof(observation));
        }

        static PendulumSystem BuildSystemAt(PendulumSystemSpec spec, ThetaObservation observation, int sampleIndex)
        {
            // 将线性样本索引还原为观测空间坐标，再映射到对应摆的初始角度。
            int[] coordinates = observation.GetCoordinates(sampleIndex);
            var pendulums = new Pendulum[spec.PendulumCount];

            for (int i = 0; i < spec.PendulumCount; i++)
            {
                pendulums[i] = new Pendulum(
                    theta: spec.DefaultThetas[i],
                    omega: spec.DefaultOmegas[i],
                    mass: spec.Mass,
                    length: spec.Length);
            }

            for (int axis = 0; axis < observation.Dimension; axis++)
            {
                int pendulumIndex = observation.StartPendulumIndex + axis;
                pendulums[pendulumIndex].Theta = observation.MapTheta(coordinates[axis]);
            }

            return new PendulumSystem(pendulums);
        }
    }
}
