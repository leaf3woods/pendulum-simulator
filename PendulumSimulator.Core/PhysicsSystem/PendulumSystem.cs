using PendulumSimulator.Core.Mathematics;

namespace PendulumSimulator.Core.PhysicsSystem
{
    /// <summary>
    /// 表示由多个摆组成的系统，提供状态向量转换、数值积分步进以及位置信息计算等方法。
    /// </summary>
    public class PendulumSystem
    {
        private readonly Pendulum[] _pendulums;
        private readonly IPendulumDynamics _dynamics;
        private readonly IStateIntegrator _integrator;

        /// <summary>
        /// 使用给定的一组摆创建一个新的摆系统。
        /// </summary>
        /// <param name="pendulums">用于初始化系统的摆集合，会被拷贝以避免外部修改影响系统状态。</param>
        /// <param name="dynamics">用于计算动力学的实现，若为 null 则使用 <see cref="MultiPendulumDynamics"/>。</param>
        /// <param name="integrator">用于数值积分的实现，若为 null 则使用 <see cref="RungeKutta4Integrator"/>。</param>
        public PendulumSystem(
            IEnumerable<Pendulum> pendulums,
            IPendulumDynamics? dynamics = null,
            IStateIntegrator? integrator = null)
        {
            _pendulums = pendulums.Select(p => p.Clone()).ToArray();

            if (_pendulums.Length == 0)
                throw new ArgumentException("A pendulum system must contain at least one pendulum.", nameof(pendulums));

            _dynamics = dynamics ?? new MultiPendulumDynamics();
            _integrator = integrator ?? new RungeKutta4Integrator();
        }

        /// <summary>
        /// 系统中摆的数量。
        /// </summary>
        public int Count => _pendulums.Length;

        /// <summary>
        /// 返回系统中摆的只读列表拷贝（底层数组的只读视图）。
        /// </summary>
        public IReadOnlyList<Pendulum> Pendulums => _pendulums;

        /// <summary>
        /// 按索引获取指定摆实例。
        /// </summary>
        public Pendulum this[int index] => _pendulums[index];

        /// <summary>
        /// 前进一步：使用当前配置的积分器和动力学计算下一个时间步的系统状态。
        /// </summary>
        /// <param name="dt">时间步长（秒），必须大于 0。</param>
        public void Step(double dt)
        {
            if (dt <= 0)
                throw new ArgumentOutOfRangeException(nameof(dt), "Time step must be greater than 0.");

            // 状态向量是积分器与动力学模型之间的通用边界：前 N 个角度，后 N 个角速度。
            double[] current = ToStateVector();
            double[] next = _integrator.Step(current, dt, state => _dynamics.Derivative(this, state));
            ApplyStateVector(next);
        }

        /// <summary>
        /// 将系统的摆角度与角速度组合为状态向量（长度为 2*N，前 N 为角度，后 N 为角速度）。
        /// </summary>
        /// <returns>返回表示当前系统状态的向量。</returns>
        public double[] ToStateVector()
        {
            int count = _pendulums.Length;
            double[] state = new double[count * 2];

            for (int i = 0; i < count; i++)
            {
                state[i] = _pendulums[i].Theta;
                state[count + i] = _pendulums[i].Omega;
            }

            return state;
        }

        /// <summary>
        /// 将状态向量应用到系统，更新每个摆的角度和角速度。
        /// </summary>
        /// <param name="state">长度为 2*N 的状态向量。</param>
        public void ApplyStateVector(IReadOnlyList<double> state)
        {
            int count = _pendulums.Length;
            if (state.Count != count * 2)
                throw new ArgumentException($"State vector must contain {count * 2} values.", nameof(state));

            for (int i = 0; i < count; i++)
            {
                _pendulums[i].Theta = MathUtilities.NormalizeAngle(state[i]);
                _pendulums[i].Omega = state[count + i];
            }
        }

        /// <summary>
        /// 计算系统中每个摆的笛卡尔坐标（相对于系统原点）。返回的数组按摆顺序排列。
        /// </summary>
        /// <returns>每个摆的 (x, y) 坐标数组。</returns>
        public (double x, double y)[] GetPositions()
        {
            var positions = new (double x, double y)[_pendulums.Length];

            double x = 0;
            double y = 0;

            for (int i = 0; i < _pendulums.Length; i++)
            {
                Pendulum pendulum = _pendulums[i];
                x += pendulum.Length * Math.Sin(pendulum.Theta);
                y += pendulum.Length * Math.Cos(pendulum.Theta);
                positions[i] = (x, y);
            }

            return positions;
        }
    }
}
