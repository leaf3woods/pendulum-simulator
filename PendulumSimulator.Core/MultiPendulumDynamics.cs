using PendulumSimulator.Core.Mathematics;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Core
{
    public sealed class MultiPendulumDynamics : IPendulumDynamics
    {
        /// <summary>
        /// 提供多摆系统的动力学计算，基于当前系统配置计算状态导数。
        /// </summary>
        public double[] Derivative(PendulumSystem system, IReadOnlyList<double> state)
        {
            int count = system.Count;
            if (state.Count != count * 2)
                throw new ArgumentException($"State vector must contain {count * 2} values.", nameof(state));

            // Derivative 只读取 system 中的质量/长度等参数；角度和角速度来自传入的候选 state。
            double[] theta = new double[count];
            double[] omega = new double[count];
            double[] tailMasses = BuildTailMasses(system);

            for (int i = 0; i < count; i++)
            {
                theta[i] = state[i];
                omega[i] = state[count + i];
            }

            double[,] massMatrix = BuildMassMatrix(system, theta, tailMasses);
            double[] rhs = BuildRightHandSide(system, theta, omega, tailMasses);
            // 质量矩阵方程 M(theta) * alpha = rhs 给出每根摆的角加速度。
            double[] alpha = LinearSystemSolver.Solve(massMatrix, rhs);

            double[] derivative = new double[count * 2];

            for (int i = 0; i < count; i++)
            {
                derivative[i] = omega[i];
                derivative[count + i] = alpha[i];
            }

            return derivative;
        }

        /// <summary>
        /// 计算每个节点到末端的累积质量（尾部质量）。
        /// </summary>
        static double[] BuildTailMasses(PendulumSystem system)
        {
            int count = system.Count;
            double[] tailMasses = new double[count];
            double runningMass = 0;

            for (int i = count - 1; i >= 0; i--)
            {
                runningMass += system[i].Mass;
                tailMasses[i] = runningMass;
            }

            return tailMasses;
        }

        /// <summary>
        /// 构建系统的质量矩阵（耦合质量项），用于求解加速度。
        /// </summary>
        static double[,] BuildMassMatrix(
            PendulumSystem system,
            IReadOnlyList<double> theta,
            IReadOnlyList<double> tailMasses)
        {
            int count = system.Count;
            double[,] matrix = new double[count, count];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    double coupledMass = tailMasses[Math.Max(i, j)];
                    matrix[i, j] =
                        coupledMass
                        * system[i].Length
                        * system[j].Length
                        * Math.Cos(theta[i] - theta[j]);
                }
            }

            return matrix;
        }

        /// <summary>
        /// 构建线性方程组右侧向量，包含重力项和速度平方的离心项。
        /// </summary>
        static double[] BuildRightHandSide(
            PendulumSystem system,
            IReadOnlyList<double> theta,
            IReadOnlyList<double> omega,
            IReadOnlyList<double> tailMasses)
        {
            int count = system.Count;
            double[] rhs = new double[count];

            for (int i = 0; i < count; i++)
            {
                double value =
                    -Physics.G
                    * system[i].Length
                    * tailMasses[i]
                    * Math.Sin(theta[i]);

                for (int j = 0; j < count; j++)
                {
                    value -=
                        tailMasses[Math.Max(i, j)]
                        * system[i].Length
                        * system[j].Length
                        * Math.Sin(theta[i] - theta[j])
                        * omega[j]
                        * omega[j];
                }

                rhs[i] = value;
            }

            return rhs;
        }
    }
}
