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
                    $"State buffer length must be {systems.Count * stateStride} for {systems.Count} pendulum systems.",
                    nameof(states));

            for (int sampleIndex = 0; sampleIndex < systems.Count; sampleIndex++)
            {
                int offset = sampleIndex * stateStride;
                var state = new double[stateStride];

                for (int stateIndex = 0; stateIndex < stateStride; stateIndex++)
                {
                    state[stateIndex] = states[offset + stateIndex];
                }

                systems[sampleIndex].ApplyStateVector(state);
            }
        }
    }
}
