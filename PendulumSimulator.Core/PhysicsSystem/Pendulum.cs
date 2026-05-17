namespace PendulumSimulator.Core.PhysicsSystem
{
    /// <summary>
    /// 表示单个摆实体。
    /// 包含角度、角速度、质量和长度等基本物理属性。
    /// </summary>
    public class Pendulum
    {
        /// <summary>
        /// 创建一个新的 <see cref="Pendulum"/> 实例。
        /// </summary>
        /// <param name="theta">初始角度（弧度）。</param>
        /// <param name="omega">初始角速度（弧度/秒）。</param>
        /// <param name="mass">摆的质量（kg），必须大于 0。</param>
        /// <param name="length">摆长（m），必须大于 0。</param>
        public Pendulum(
            double theta = 0.0,
            double omega = 0.0,
            double mass = 1.0,
            double length = 1.0)
        {
            if (mass <= 0)
                throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be greater than 0.");
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");

            Theta = theta;
            Omega = omega;
            Mass = mass;
            Length = length;
        }

        /// <summary>
        /// 摆的角度（弧度）。
        /// </summary>
        public double Theta { get; set; }

        /// <summary>
        /// 摆的角速度（弧度/秒）。
        /// </summary>
        public double Omega { get; set; }

        /// <summary>
        /// 摆的质量（kg）。只读。
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// 摆长（m）。只读。
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// 创建当前摆的浅拷贝。
        /// </summary>
        /// <returns>返回一个新的 <see cref="Pendulum"/> 实例，包含相同的属性值。</returns>
        public Pendulum Clone()
        {
            return new Pendulum(Theta, Omega, Mass, Length);
        }
    }
}
