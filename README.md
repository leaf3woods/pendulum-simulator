# Pendulum Simulator

多摆系统的物理模拟与混乱/稳定分布分析项目。

Physics simulation and chaos/stability distribution analysis for multi-pendulum systems.

## 我们要实现什么 / What We Are Building

我们要实现的是一个可以研究 **N 摆系统初始角度空间** 的分析系统。

We are building a system for studying the **initial-angle parameter space** of an N-pendulum system.

核心目标不是先做动画，而是构建：

The primary goal is not animation first, but to build:

- 一个可模拟任意数量摆的物理核心。
- A physics core that can simulate an arbitrary number of pendulums.
- 一个可以批量生成 N 摆系统的场。
- A field that can generate and hold many N-pendulum systems.
- 一个可以从高维初始角度空间中取样的分析层。
- An analysis layer that samples from a high-dimensional initial-angle space.
- 一个后续可以输出混乱/稳定分布图的数据基础。
- A data foundation for future chaos/stability distribution maps.

## 核心概念 / Core Concepts

### N 摆系统 / N-Pendulum System

一个 `PendulumSystem` 表示一个由 N 个摆组成的串联系统。

A `PendulumSystem` represents one serial system made of N pendulums.

每个摆有：

Each pendulum has:

- 角度 `theta`
- angle `theta`
- 角速度 `omega`
- angular velocity `omega`
- 质量 `mass`
- mass
- 长度 `length`
- length

当前分析目标中，初始角速度统一为 `0`，我们主要观察初始角度。

For the current analysis goal, initial angular velocity is fixed to `0`; we mainly observe initial angles.

### 系统蓝图 / System Spec

`PendulumSystemSpec` 描述 "这是一个什么样的 N 摆系统"，与"如何观察它"完全解耦。

`PendulumSystemSpec` describes "what kind of N-pendulum system this is", fully decoupled from "how we observe it".

包含字段：

Contains:

- `PendulumCount`
- `Mass`、`Length`（当前为均质：所有摆共用）
- `Mass`, `Length` (currently uniform: shared by all pendulums)
- `DefaultThetas`、`DefaultOmegas`（每个摆的默认初值，长度 = `PendulumCount`）
- `DefaultThetas`, `DefaultOmegas` (per-pendulum defaults, length = `PendulumCount`)

未被观察轴扫描的摆，使用 spec 中的默认初值。

Pendulums not swept by an observation use these defaults.

### 系统场 / System Field

`PendulumSystemField` 表示一批 N 摆系统。

`PendulumSystemField` represents a batch of N-pendulum systems.

它只负责：

It only handles:

- 持有一定数量的 `PendulumSystem`
- holding a number of `PendulumSystem` instances
- 统一推进所有系统
- advancing all systems together

它不关心：

It does not know about:

- 二维或三维
- 2D or 3D
- 像素
- pixels
- 观察窗口
- observation windows
- 可视化
- visualization
- 哪些摆的角度被观察
- which pendulum angles are being observed

也就是说：

In other words:

```text
Field = many N-pendulum systems + Step(dt)
```

### 初始角度参数空间 / Initial-Angle Parameter Space

一个 N 摆系统有 N 个初始角度：

An N-pendulum system has N initial angles:

```text
theta0, theta1, theta2, ..., thetaN-1
```

这些角度组成一个 N 维参数空间。`AngleObservation` 描述要扫描这个空间的哪些轴、范围、分辨率。

These angles form an N-dimensional parameter space. `AngleObservation` describes which axes of this space to sweep, with what range and resolution.

字段：

Fields:

- `StartPendulumIndex`：从第几根摆开始扫描
- `StartPendulumIndex`: which pendulum index to start sweeping from
- `Dimension`：连续扫描多少根摆的角度（**任意 N**）
- `Dimension`: how many contiguous angles to sweep (**arbitrary N**)
- `Resolution`：每根观察轴的采样数
- `Resolution`: samples per observed axis
- `ThetaMin`、`ThetaMax`：所有观察轴共享的角度范围
- `ThetaMin`, `ThetaMax`: angle range shared by every observed axis

例如：

For example:

```text
1D: theta[n]
2D: theta[n], theta[n+1]
3D: theta[n], theta[n+1], theta[n+2]
kD: theta[n], theta[n+1], ..., theta[n+k-1]
```

二维和三维只是更容易观察的窗口，不是分析本身。分析层在维度上完全通用。

2D and 3D are only easier-to-observe windows, not the analysis itself. The analysis layer is fully dimension-agnostic.

### 设计原则：扫描永远是"范围 + 分辨率"，不是"任意列表" / Design Principle: Sweeps Are Always Range + Resolution, Never Arbitrary Lists

观察轴上的采样点是一个均匀网格 `Resolution^Dimension`，绝不是"用户传一串具体的角度值"。

Observed-axis samples form a uniform `Resolution^Dimension` grid; users never pass an arbitrary list of specific values.

判断字段归属的简单规则：

A simple rule for placing a field:

- 数量级与 `PendulumCount` 一致（小、O(N摆数)） → 放 `PendulumSystemSpec`，可以是列表
- Scales with `PendulumCount` (small, O(pendulum count)) → goes in `PendulumSystemSpec`, may be a list
- 数量级与 `Resolution^Dimension` 一致（可达千万级） → 必须用范围参数化，放 `AngleObservation`
- Scales with `Resolution^Dimension` (potentially tens of millions) → must be parameterised by range, goes in `AngleObservation`

未来扩展（如扫描质量、初始速度）也遵循同一原则。

Future extensions (scanning mass, initial velocity, ...) follow the same principle.

## 数据流 / Data Flow

```text
1. Define a PendulumSystemSpec (the N-pendulum blueprint)
2. Define one or more AngleObservations describing which axes to sweep
3. PendulumSystemFieldFactory.Build(spec, observation) -> PendulumSystemField
4. Step the field forward in time (field.Step(dt))
5. Analyze the resulting systems (IFieldAnalyzer<T> -> NdArray<T>)
6. Let a view render the analysis or the field directly
```

中文表述：

```text
1. 定义一个 PendulumSystemSpec（N 摆系统蓝图）
2. 定义一个或多个 AngleObservation 描述要扫描哪些轴
3. PendulumSystemFieldFactory.Build(spec, observation) → PendulumSystemField
4. 让整个场随时间推进（field.Step(dt)）
5. 在场上跑分析（IFieldAnalyzer<T> → NdArray<T>）
6. 由 View 渲染分析结果或直接渲染 field
```

## 使用示例 / Usage Example

```csharp
// 1. Blueprint: a 5-pendulum system, uniform mass/length, all hanging down at t=0
var spec = PendulumSystemSpec.Uniform(pendulumCount: 5);

// 2. Two independent observations on the same spec
var obs2d = new AngleObservation
{
    StartPendulumIndex = 1,
    Dimension = 2,
    Resolution = 128,
    ThetaMin = -Math.PI,
    ThetaMax = Math.PI,
};
var obs3d = obs2d with { Dimension = 3, Resolution = 64 };

// 3. Build a field per observation
PendulumSystemField field2d = PendulumSystemFieldFactory.Build(spec, obs2d);
PendulumSystemField field3d = PendulumSystemFieldFactory.Build(spec, obs3d);

// 4. Render
new Video2DView(Video2DViewOptions.Default).Run(field2d, obs2d);
new Builder3DView(Builder3DViewOptions.Default).Run(field3d, obs3d);
```

## 模块边界 / Module Boundaries

### Core

Core 负责物理本身。

Core owns the physics.

包含：

Contains:

- `Pendulum`
- `PendulumSystem`、`PendulumSystemField`
- `PendulumSystem`, `PendulumSystemField`
- N 摆动力学（`MultiPendulumDynamics`）
- N-pendulum dynamics (`MultiPendulumDynamics`)
- 数值积分器（`RungeKutta4Integrator`、`LinearSystemSolver`）
- numerical integrators (`RungeKutta4Integrator`, `LinearSystemSolver`)

不包含：

Does not contain:

- 参数空间采样
- parameter-space sampling
- 观察窗口
- observation windows
- 颜色映射
- color mapping
- 视频或 UI
- video or UI

### Analysis

Analysis 负责参数空间和分析。

Analysis owns parameter spaces and analysis.

包含：

Contains:

- `PendulumSystemSpec`（系统蓝图）
- `PendulumSystemSpec` (system blueprint)
- `AngleObservation`（任意 N 维扫描描述）
- `AngleObservation` (arbitrary-dimensional sweep description)
- `PendulumSystemFieldFactory`（spec + observation → field）
- `PendulumSystemFieldFactory` (spec + observation → field)
- `IFieldAnalyzer<T>` 和 `NdArray<T>`（分析骨架，待具体实现）
- `IFieldAnalyzer<T>` and `NdArray<T>` (analysis scaffolding; concrete analyzers TBD)

不包含：

Does not contain:

- 维度限制（Analysis 在维度上通用，View 层才有 2D/3D 限制）
- dimension constraints (Analysis is dimension-agnostic; only the View layer is dimension-restricted)
- 渲染或 UI
- rendering or UI

### Viewer

Viewer 只负责渲染。

Viewer only renders.

包含：

Contains:

- 按维度分的视图接口：`IPendulum1DView`、`IPendulum2DView`、`IPendulum3DView`
- Dimension-restricted view interfaces: `IPendulum1DView`, `IPendulum2DView`, `IPendulum3DView`
- 具体视图：`Console2DView`、`Video2DView`、`Builder3DView`（3D 当前为占位）
- Concrete views: `Console2DView`, `Video2DView`, `Builder3DView` (3D is a stub for now)
- 每个视图的配置，含一个组合进来的 `RenderOptions`
- Per-view options, each composing a shared `RenderOptions`
- 颜色映射：`PendulumStateColorMap`、`PendulumColorScheme`
- Color mapping: `PendulumStateColorMap`, `PendulumColorScheme`

它消费 Analysis 的输出，不参与物理建模。

It consumes Analysis output and does not participate in physics modelling.

每个视图在 `Run` 入口检查 `observation.Dimension`，2D/3D 不匹配的输入会被早期拒绝。

Each view verifies `observation.Dimension` at the entry of `Run`; mismatched 2D/3D inputs are rejected early.

## 暂时不做什么 / What We Are Not Prioritizing Yet

当前阶段暂时不优先处理：

Not prioritized at this stage:

- 高维 scan 的切片可视化（N 维 Analysis → 2D/3D 切片渲染）
- Slicing visualisation of high-dimensional scans (N-dim Analysis → 2D/3D slice rendering)
- 真正的 3D 体素渲染（当前 `Builder3DView` 只打印元信息）
- A real volumetric 3D renderer (current `Builder3DView` only prints metadata)
- 非均质系统（每摆不同质量/长度的 spec 暴露）
- Non-uniform systems (per-pendulum mass/length exposed on the spec)
- 具体的稳定/混沌分析器实现
- Concrete stability/chaos analyzer implementations
- 高级颜色映射（如 HSV、按邻域方差着色）
- Advanced color mapping (e.g., HSV, neighbour-variance shading)

这些都应该在物理系统、分析层骨架和视图骨架稳定之后再处理。

These come after the physics, the analysis scaffolding, and the view scaffolding stabilise.

## 当前设计判断 / Current Design Decisions

- 初始角速度固定为 `0`。
- Initial angular velocity is fixed to `0`.
- "系统是什么"（`PendulumSystemSpec`）与"怎么观察"（`AngleObservation`）严格分离。
- "What the system is" (`PendulumSystemSpec`) is strictly separated from "how it is observed" (`AngleObservation`).
- 观察轴恒为"范围 + 分辨率"，不接受任意列表。
- Observed axes are always specified as "range + resolution", never arbitrary lists.
- Analysis 层维度无关；维度限制只在 View 边界做运行时检查。
- The Analysis layer is dimension-agnostic; dimension limits live only at the View boundary as runtime checks.
- View 多态用按维度划分的接口（`IPendulum{1,2,3}DView`），编译期阻止跨维度调用。
- View polymorphism uses dimension-specific interfaces (`IPendulum{1,2,3}DView`), preventing cross-dimensional calls at compile time.
- View 配置用组合（共享 `RenderOptions`），不用 record 继承。
- View options use composition (a shared `RenderOptions` member), not record inheritance.
- 质量与长度当前为单值（均质摆）；物理层早已支持非均质，扩展是纯 API 改动。
- Mass and length are single values for now (uniform pendulums); the physics layer already supports non-uniform configurations, so extending it is a pure API change.

## 后续方向 / Next Steps

接下来可优先完善：

Next, we can focus on:

1. 具体的 `IFieldAnalyzer<T>` 实现（如 time-to-flip、能量阈值分类、Lyapunov 估计）。
1. Concrete `IFieldAnalyzer<T>` implementations (time-to-flip, energy-threshold classifier, Lyapunov estimator).
2. 让 View 消费 `NdArray<T>` 的分析结果，而不是直接读 field 当前状态。
2. Let views consume `NdArray<T>` analysis results instead of reading the field's current state directly.
3. 改进 2D 渲染的色彩映射，使稳定区与混沌区在视觉上对比更强（如 HSV 色相方案）。
3. Improve the 2D color mapping so stable and chaotic regions have stronger visual contrast (e.g., HSV hue scheme).
4. 实现真正的 3D 体素渲染替换当前的 `Builder3DView` 占位。
4. Replace the `Builder3DView` stub with a real volumetric renderer.
5. 继续审查 N 摆动力学模型的数学正确性。
5. Continue reviewing the mathematical correctness of the N-pendulum dynamics model.
