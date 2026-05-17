using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.ThreeDimension
{
    public record Builder3DViewOptions
    {
        public static Builder3DViewOptions Default { get; } = new();

        public RenderOptions Render { get; init; } = new();
    }
}
