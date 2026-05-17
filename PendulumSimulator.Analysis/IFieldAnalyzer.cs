using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Analysis
{
    /// <summary>
    /// 对 <see cref="PendulumSystemField"/> 的每个样本计算度量，产生由源 <see cref="ThetaObservation"/> 形状决定的结果数组。
    /// </summary>
    /// <remarks>
    /// 占位接口。具体的分析器（翻转时间、Lyapunov 估计器、能量分类器等）尚未实现。
    /// </remarks>
    public interface IFieldAnalyzer<TResult>
    {
        NdArray<TResult> Analyze(PendulumSystemField field, IObservation<double> observation);
    }
}
