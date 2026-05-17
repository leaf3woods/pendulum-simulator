using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Video
{
    public record Video2DViewOptions
    {
        public static Video2DViewOptions Default { get; } = new();

        public RenderOptions Render { get; init; } = new();

        public string OutputPath { get; init; } = "pendulum_2d.mp4";

        public int Fps { get; init; } = 30;

        public int DurationSeconds { get; init; } = 20;

        public string Codec { get; init; } = "mp4v";

        public int FrameCount => Fps * DurationSeconds;
    }
}
