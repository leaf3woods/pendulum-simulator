using PendulumSimulator.Core.Mathematics;
using Xunit;

namespace PendulumSimulator.Tests.Core.Mathematics
{
    public class LinearSystemSolverTests
    {
        [Fact]
        public void SolveReturnsExpectedVector()
        {
            double[,] matrix =
            {
                { 3.0, 2.0 },
                { 1.0, 2.0 }
            };
            double[] values = [5.0, 5.0];

            double[] solution = LinearSystemSolver.Solve(matrix, values);

            Assert.Equal(0.0, solution[0], precision: 12);
            Assert.Equal(2.5, solution[1], precision: 12);
        }

        [Fact]
        public void SolveThrowsForSingularMatrix()
        {
            double[,] matrix =
            {
                { 1.0, 2.0 },
                { 2.0, 4.0 }
            };

            Assert.Throws<InvalidOperationException>(() =>
                LinearSystemSolver.Solve(matrix, [3.0, 6.0]));
        }
    }
}
