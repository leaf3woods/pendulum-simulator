using PendulumSimulator.Viewer.Options;
using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer
{
    public class SimulatorConfiguration
    {
        public string? OutputDirectory { get; init; }

        public ViewerMode? Mode { get; init; }

        public PendulumColorScheme? ColorScheme { get; set; }

        public ObservationConfiguration? Observation { get; set; }

        public SystemSpecConfiguration? SystemSpec { get; set; }

        public RenderConfiguration? Render { get; init; }

        public int Fps { get; init; } = 30;

        public int DurationSeconds { get; init; } = 20;

        public static SimulatorConfiguration Example { get; } = new()
        {
            Mode = ViewerMode.Video,
            ColorScheme = PendulumColorScheme.RgbAngles,
            SystemSpec = new SystemSpecConfiguration
            {
                PendulumCount = 3,
                Mass = 1.0,
                Length = 1.0
            },
            Observation = new ObservationConfiguration
            {
                Dimension = 2,
                Resolution = 512
            },
            Render = new RenderConfiguration
            {
                TimeStep = 0.01,
                SimulationStepsPerFrame = 10
            }
        };
    }

    public class ObservationConfiguration
    {
        public int StartPendulumIndex { get; set; }
        public int Dimension { get; set; }
        public int Resolution { get; set; }
    }

    public class SystemSpecConfiguration
    {
        public int PendulumCount { get; set; }
        public double Mass { get; set; }
        public double Length { get; set; }
    }

    public class RenderConfiguration
    {
        public double TimeStep { get; init; }

        public int SimulationStepsPerFrame { get; init; }
    }
}
