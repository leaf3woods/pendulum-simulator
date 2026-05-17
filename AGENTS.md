# Repository Guidelines

## Project Structure & Module Organization

This .NET 10 solution is organized by responsibility:

- `PendulumSimulator.Core/` contains physics models, multi-pendulum dynamics, and numerical math utilities.
- `PendulumSimulator.Analysis/` builds observations, system specs, fields, and arrays over the core simulation.
- `PendulumSimulator.Viewer/` is the executable project for console, 3D builder, and video rendering modes. Runtime configuration lives in `appsettings*.json`.
- `PendulumSimulator.Tests/` contains xUnit tests grouped by source area, for example `Core/PhysicsSystem` and `Analysis`.

Keep code in the layer that owns the concept. Core should not depend on Analysis or Viewer; Analysis may depend on Core; Viewer may depend on Analysis.

## Build, Test, and Development Commands

- `dotnet build PendulumSimulator.slnx` compiles all projects.
- `dotnet test PendulumSimulator.slnx` runs the xUnit test suite.
- `dotnet run --project PendulumSimulator.Viewer -- --render console` runs the realtime console view.
- `dotnet run --project PendulumSimulator.Viewer -- --render 3d` runs the 3D builder view.
- `dotnet run --project PendulumSimulator.Viewer -- --render video` generates video output using viewer settings.

Viewer color schemes can be selected with `--color rgb` or `--color grayscale`.

## Coding Style & Naming Conventions

Use C# with nullable reference types and implicit usings enabled. Follow the existing style: four-space indentation, PascalCase for public types and members, camelCase for locals and parameters, and `I` prefixes for interfaces such as `IStateIntegrator`.

Prefer immutable records for configuration-like values when they match existing patterns. Keep comments short and limited to non-obvious behavior.

## Testing Guidelines

Tests use xUnit with `Microsoft.NET.Test.Sdk` and `coverlet.collector`. Place tests under `PendulumSimulator.Tests/` mirroring the source folder. Name test classes after the subject, such as `PendulumSystemTests`, and use descriptive test method names.

Run `dotnet test PendulumSimulator.slnx` before submitting changes. Add tests for numerical edge cases, invalid dimensions, and Core/Analysis boundary behavior.

## Commit & Pull Request Guidelines

The current history only contains an initial commit, so no strict convention is established. Use short imperative messages, for example `Add RK4 edge case tests`.

Pull requests should include a concise description, test results, and configuration or rendering impact. For Viewer changes, include screenshots, sample output paths, or verification commands.

## Configuration Notes

Do not commit generated binaries or large render outputs. Keep reusable defaults in `PendulumSimulator.Viewer/appsettings*.json`, and document any new required settings in `README.md` or this guide.
