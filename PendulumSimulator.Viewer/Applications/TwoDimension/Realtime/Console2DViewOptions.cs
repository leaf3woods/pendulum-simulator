using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Realtime
{
    public record Console2DViewOptions
    {
        public static Console2DViewOptions Default { get; } = new()
        {
            Render = new RenderOptions
            {
                SimulationStepsPerFrame = 5,
            },
        };

        public RenderOptions Render { get; init; } = new();

        public int Fps { get; init; } = 12;
    }
}
