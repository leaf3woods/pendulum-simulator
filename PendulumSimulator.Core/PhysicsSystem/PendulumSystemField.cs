using PendulumSimulator.Core.GpuShader;

namespace PendulumSimulator.Core.PhysicsSystem
{
    /// <summary>
    /// 保存一批摆系统并统一推进它们的仿真。
    /// </summary>
    public sealed class PendulumSystemField
    {
        private readonly PendulumSystem[] _systems;

        public PendulumSystemField(IEnumerable<PendulumSystem> systems)
        {
            _systems = systems?.ToArray() ?? throw new ArgumentNullException(nameof(systems));

            if (_systems.Length == 0)
                throw new ArgumentException("A pendulum system field must contain at least one system.", nameof(systems));

            PendulumCount = _systems[0].Count;
            if (_systems.Any(system => system.Count != PendulumCount))
                throw new ArgumentException("All systems in a field must have the same pendulum count.", nameof(systems));
        }

        public int Count => _systems.Length;

        public int PendulumCount { get; }

        public IReadOnlyList<PendulumSystem> Systems => _systems;

        public PendulumSystem this[int index] => _systems[index];

        public void Step(double dt, int steps = 1, bool useGpu = false)
        {
            if (steps <= 0)
                throw new ArgumentOutOfRangeException(nameof(steps), "Step count must be greater than 0.");
            if (!useGpu)
            {
                // 以物理步为外层循环，确保整片场在进入下一步前都停在同一个模拟时间。
                for (int step = 0; step < steps; step++)
                {
                    Parallel.ForEach(_systems, system =>
                    {
                        system.Step(dt);
                    });
                }
            }
            else
            {
                using var shaderField = new PendulumFieldGpuRunner(this);
                shaderField.Step((float)dt, steps);
                this.ApplyStates(shaderField.CopyStatesToCpu());
            }
        }
    }
}
