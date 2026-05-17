using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Options
{
    public record ViewerCommand(ViewerMode? Mode, PendulumColorScheme? ColorScheme)
    {
        public static ViewerCommand? Parse(IReadOnlyList<string> args)
        {
            ViewerMode? mode = ViewerMode.Video;
            PendulumColorScheme? colorScheme = PendulumColorScheme.RgbAngles;
            if(args.Count == 0)
            {
                return null;
            }
            var configRender = false;
            var configColor = false;

            foreach (string arg in args)
            {
                if(arg.Equals("-r", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--render", StringComparison.OrdinalIgnoreCase))
                {
                    configRender = true;
                }

                if (arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--color", StringComparison.OrdinalIgnoreCase))
                {
                    configColor = true;
                }

                if (configRender)
                {
                    if (arg.Equals("console", StringComparison.OrdinalIgnoreCase))
                        mode = ViewerMode.Console;
                    else if (arg.Equals("3d", StringComparison.OrdinalIgnoreCase))
                        mode = ViewerMode.ThreeDimensional;
                    else if (arg.Equals("video", StringComparison.OrdinalIgnoreCase))
                        mode = ViewerMode.Video;
                }

                if (configColor)
                {
                    if (arg.Equals("rgb", StringComparison.OrdinalIgnoreCase))
                        colorScheme = PendulumColorScheme.RgbAngles;
                    else if (arg.Equals("grayscale", StringComparison.OrdinalIgnoreCase))
                        colorScheme = PendulumColorScheme.GrayscaleAngles;
                }
            }

            return new ViewerCommand(mode, colorScheme);
        }
    }
}
