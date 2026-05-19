using ComputeSharp;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Core.GpuShader
{
    public class PendulumFieldGpuRunner : IDisposable
    {
        private const int DispatchTileWidth = 4096;

        private readonly GraphicsDevice _device;
        private readonly ReadWriteBuffer<float> _states;
        private readonly PendulumSystemField _systemField;
        private readonly PendulumSystem _defaultPendulumSystem;

        private bool _disposed;

        public PendulumFieldGpuRunner(PendulumSystemField systemField)
        {
            _systemField = systemField;
            _defaultPendulumSystem = systemField[0];

            if (systemField.Count == 0 || !IsSupportedPendulumCount(_defaultPendulumSystem.Count))
                throw new ArgumentException("GPU stepping supports 2, 3, or 4 pendulums.", nameof(systemField));
            var states = new float[systemField.Count * StateStride];

            for (int sampleIndex = 0; sampleIndex < systemField.Count; sampleIndex++)
            {
                double[] state = systemField[sampleIndex].ToStateVector();
                int offset = sampleIndex * StateStride;

                for (int stateIndex = 0; stateIndex < StateStride; stateIndex++)
                {
                    states[offset + stateIndex] = (float)state[stateIndex];
                }
            }

            _device = GraphicsDevice.GetDefault();
            _states = _device.AllocateReadWriteBuffer(states.ToArray());
        }

        public static bool IsSupportedPendulumCount(int pendulumCount)
        {
            return pendulumCount is >= 2 and <= 4;
        }

        public GraphicsDevice Device
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _device;
            }
        }

        public ReadWriteBuffer<float> States
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _states;
            }
        }

        public int Count => _systemField.Count;

        public int PendulumCount => _systemField.PendulumCount;

        public int StateStride => PendulumCount * 2;

        public (int Width, int Height) DispatchSize
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                int dispatchWidth = Math.Min(_systemField.Count, DispatchTileWidth);
                int dispatchHeight = (_systemField.Count + dispatchWidth - 1) / dispatchWidth;
                return (dispatchWidth, dispatchHeight);
            }
        }


        public void Step(float dt, int steps = 1)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (dt <= 0)
                throw new ArgumentOutOfRangeException(nameof(dt), "Time step must be greater than 0.");
            if (steps <= 0)
                throw new ArgumentOutOfRangeException(nameof(steps), "Step count must be greater than 0.");

            var (dispatchWidth, dispatchHeight) = DispatchSize;

            switch (PendulumCount)
            {
                case 2:
                    _device.For(
                        dispatchWidth,
                        dispatchHeight,
                        new DoublePendulumStepShader(
                            _states,
                            (float)_defaultPendulumSystem[0].Mass,
                            (float)_defaultPendulumSystem[1].Mass,
                            (float)_defaultPendulumSystem[0].Length,
                            (float)_defaultPendulumSystem[1].Length,
                            dt,
                            steps,
                            Count));
                    break;

                case 3:
                    _device.For(
                        dispatchWidth,
                        dispatchHeight,
                        new TriplePendulumStepShader(
                            _states,
                            (float)_defaultPendulumSystem[0].Mass,
                            (float)_defaultPendulumSystem[1].Mass,
                            (float)_defaultPendulumSystem[2].Mass,
                            (float)_defaultPendulumSystem[0].Length,
                            (float)_defaultPendulumSystem[1].Length,
                            (float)_defaultPendulumSystem[2].Length,
                            dt,
                            steps,
                            Count));
                    break;

                case 4:
                    _device.For(
                        dispatchWidth,
                        dispatchHeight,
                        new QuadruplePendulumStepShader(
                            _states,
                            (float)_defaultPendulumSystem[0].Mass,
                            (float)_defaultPendulumSystem[1].Mass,
                            (float)_defaultPendulumSystem[2].Mass,
                            (float)_defaultPendulumSystem[3].Mass,
                            (float)_defaultPendulumSystem[0].Length,
                            (float)_defaultPendulumSystem[1].Length,
                            (float)_defaultPendulumSystem[2].Length,
                            (float)_defaultPendulumSystem[3].Length,
                            dt,
                            steps,
                            Count));
                    break;

                default:
                    throw new NotSupportedException($"GPU stepping does not support {PendulumCount} pendulums.");
            }
        }

        public float[] CopyStatesToCpu()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var states = new float[_states.Length];
            _states.CopyTo(states);
            return states;
        }

        public void CopyStatesTo()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(_systemField);

            _systemField.ApplyStates(CopyStatesToCpu());
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _states.Dispose();
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
