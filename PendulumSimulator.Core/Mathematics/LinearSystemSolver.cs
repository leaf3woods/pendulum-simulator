namespace PendulumSimulator.Core.Mathematics
{
    /// <summary>
    /// 提供求解线性方程组的简单高斯消元实现，主要用于求解摆系统的质量矩阵方程。
    /// </summary>
    public static class LinearSystemSolver
    {
        /// <summary>
        /// 使用带列主元选择的高斯消元法求解线性方程组 Ax = b。
        /// </summary>
        /// <param name="coefficients">系数矩阵 A（必须为方阵，大小为 N x N）。</param>
        /// <param name="values">右侧向量 b，长度为 N。</param>
        /// <returns>返回解向量 x，长度为 N。</returns>
        /// <exception cref="ArgumentException">当系数矩阵不是方阵或其大小与值向量长度不匹配时抛出。</exception>
        /// <exception cref="InvalidOperationException">当矩阵奇异无法求解时抛出。</exception>
        public static double[] Solve(double[,] coefficients, IReadOnlyList<double> values)
        {
            int size = values.Count;
            if (coefficients.GetLength(0) != size || coefficients.GetLength(1) != size)
                throw new ArgumentException("Coefficient matrix must be square and match the value vector length.");

            double[,] matrix = (double[,])coefficients.Clone();
            double[] rhs = new double[size];
            for (int i = 0; i < size; i++)
            {
                rhs[i] = values[i];
            }

            for (int column = 0; column < size; column++)
            {
                int pivotRow = FindPivotRow(matrix, column);
                if (Math.Abs(matrix[pivotRow, column]) < 1e-12)
                    throw new InvalidOperationException("The pendulum mass matrix is singular.");

                if (pivotRow != column)
                {
                    SwapRows(matrix, rhs, column, pivotRow);
                }

                double pivot = matrix[column, column];

                for (int row = column + 1; row < size; row++)
                {
                    double factor = matrix[row, column] / pivot;
                    matrix[row, column] = 0;

                    for (int col = column + 1; col < size; col++)
                    {
                        matrix[row, col] -= factor * matrix[column, col];
                    }

                    rhs[row] -= factor * rhs[column];
                }
            }

            return BackSubstitute(matrix, rhs);
        }

        /// <summary>
        /// 在给定列中查找具有最大绝对值的主元行（用于列主元选择）。
        /// </summary>
        static int FindPivotRow(double[,] matrix, int column)
        {
            int size = matrix.GetLength(0);
            int pivotRow = column;
            double pivotValue = Math.Abs(matrix[column, column]);

            for (int row = column + 1; row < size; row++)
            {
                double candidate = Math.Abs(matrix[row, column]);
                if (candidate > pivotValue)
                {
                    pivotValue = candidate;
                    pivotRow = row;
                }
            }

            return pivotRow;
        }

        /// <summary>
        /// 交换矩阵的两行以及对应的右侧向量元素。
        /// </summary>
        static void SwapRows(double[,] matrix, double[] rhs, int first, int second)
        {
            int size = matrix.GetLength(1);

            for (int col = 0; col < size; col++)
            {
                (matrix[first, col], matrix[second, col]) = (matrix[second, col], matrix[first, col]);
            }

            (rhs[first], rhs[second]) = (rhs[second], rhs[first]);
        }

        /// <summary>
        /// 对上三角矩阵执行回代，求解目标向量。
        /// </summary>
        static double[] BackSubstitute(double[,] matrix, double[] rhs)
        {
            int size = rhs.Length;
            double[] solution = new double[size];

            for (int row = size - 1; row >= 0; row--)
            {
                double value = rhs[row];

                for (int col = row + 1; col < size; col++)
                {
                    value -= matrix[row, col] * solution[col];
                }

                solution[row] = value / matrix[row, row];
            }

            return solution;
        }
    }
}
