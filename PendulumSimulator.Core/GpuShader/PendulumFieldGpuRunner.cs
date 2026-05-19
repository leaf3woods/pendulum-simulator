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

            if (systemField.Count == 0 || _defaultPendulumSystem.Count != 2)
                throw new ArgumentException("invalid pendulum systems.", nameof(systemField));
            var states = new float[systemField.Count * systemField.PendulumCount * 2];

            for (int sampleIndex = 0; sampleIndex < systemField.Count; sampleIndex++)
            {
                double[] state = systemField[sampleIndex].ToStateVector();
                int offset = sampleIndex * systemField.PendulumCount * 2;

                states[offset + 0] = (float)state[0];
                states[offset + 1] = (float)state[1];
                states[offset + 2] = (float)state[2];
                states[offset + 3] = (float)state[3];
            }

            _device = GraphicsDevice.GetDefault();
            _states = _device.AllocateReadWriteBuffer(states.ToArray());
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
