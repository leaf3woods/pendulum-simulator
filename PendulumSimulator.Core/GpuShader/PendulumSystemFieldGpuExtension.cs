using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Core.GpuShader
{
    public static class PendulumSystemFieldGpuExtension
    {
        public static void ApplyStates(this PendulumSystemField field, IReadOnlyList<float> states)
        {
            var systems = field.Systems;
            var stateStride = systems[0].Count * 2;

            if (states.Count != systems.Count * stateStride)
                throw new ArgumentException(
                    $"State buffer length must be {systems.Count * stateStride} for {systems.Count} double pendulum systems.",
                    nameof(states));

            for (int sampleIndex = 0; sampleIndex < systems.Count; sampleIndex++)
            {
                int offset = sampleIndex * stateStride;

                systems[sampleIndex].ApplyStateVector(
                [
                    states[offset + 0],
                    states[offset + 1],
                    states[offset + 2],
                    states[offset + 3]
                ]);
            }
        }
    }
}
