using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Core
{
    public interface IPendulumDynamics
    {
        /// <summary>
        /// 计算系统状态的导数（状态向量的时间导数）。
        /// </summary>
        /// <param name="system">所属的 <see cref="PendulumSystem"/> 实例，用于查询摆的参数等信息。</param>
        /// <param name="state">长度为 2*N 的状态向量，前 N 个元素为角度，后 N 个元素为角速度。</param>
        /// <returns>返回与输入状态向量等长的导数向量。</returns>
        double[] Derivative(PendulumSystem system, IReadOnlyList<double> state);
    }
}
