namespace PendulumSimulator.Core.Mathematics
{
    public sealed class RungeKutta4Integrator : IStateIntegrator
    {
        /// <summary>
        /// 使用经典四阶 Runge-Kutta 方法对状态向量执行单步积分。
        /// </summary>
        /// <param name="state">当前状态向量。</param>
        /// <param name="dt">时间步长（秒）。</param>
        /// <param name="derivative">用于计算状态导数的函数。</param>
        /// <returns>返回下一时刻的状态向量。</returns>
        public double[] Step(double[] state, double dt, Func<double[], double[]> derivative)
        {
            // RK4 使用四次斜率估计来换取更高精度；这也是批量渲染中的主要计算放大项。
            double[] k1 = derivative(state);
            double[] k2 = derivative(Add(state, Scale(k1, dt / 2.0)));
            double[] k3 = derivative(Add(state, Scale(k2, dt / 2.0)));
            double[] k4 = derivative(Add(state, Scale(k3, dt)));

            double[] next = new double[state.Length];

            for (int i = 0; i < state.Length; i++)
            {
                next[i] =
                    state[i]
                    + dt / 6.0 * (k1[i] + 2.0 * k2[i] + 2.0 * k3[i] + k4[i]);
            }

            return next;
        }

        /// <summary>
        /// 向量加法。
        /// </summary>
        static double[] Add(IReadOnlyList<double> a, IReadOnlyList<double> b)
        {
            double[] result = new double[a.Count];

            for (int i = 0; i < a.Count; i++)
            {
                result[i] = a[i] + b[i];
            }

            return result;
        }

        /// <summary>
        /// 将向量按标量缩放。
        /// </summary>
        static double[] Scale(IReadOnlyList<double> values, double scalar)
        {
            double[] result = new double[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                result[i] = values[i] * scalar;
            }

            return result;
        }
    }
}
