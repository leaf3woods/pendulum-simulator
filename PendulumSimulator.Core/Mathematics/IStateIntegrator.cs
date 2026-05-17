namespace PendulumSimulator.Core.Mathematics
{
    public interface IStateIntegrator
    {
        /// <summary>
        /// 执行一个时间步的数值积分，返回下一时刻的状态向量。
        /// </summary>
        /// <param name="state">当前状态向量。</param>
        /// <param name="dt">时间步长（秒），必须大于 0。</param>
        /// <param name="derivative">用于计算状态导数的函数，签名为 Func&lt;double[], double[]&gt;。</param>
        /// <returns>返回下一时刻的状态向量。</returns>
        double[] Step(double[] state, double dt, Func<double[], double[]> derivative);
    }
}
