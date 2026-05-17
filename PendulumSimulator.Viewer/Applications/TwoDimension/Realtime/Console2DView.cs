using System.Text;
using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.PhysicsSystem;
using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Realtime
{
    public class Console2DView : IPendulum2DView
    {
        static readonly char[] Symbols = " .:-=+*#%@".ToCharArray();

        private readonly Console2DViewOptions _options;

        public Console2DView(Console2DViewOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (options.Fps <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Fps must be greater than 0.");
            _options = options;
        }

        public void Run(PendulumSystemField field, ThetaObservation observation)
        {
            ArgumentNullException.ThrowIfNull(field);
            ArgumentNullException.ThrowIfNull(observation);

            if (observation.Dimension != 2)
                throw new ArgumentException(
                    $"Console2DView requires a 2D observation; got Dimension={observation.Dimension}.",
                    nameof(observation));

            int resolution = observation.Resolution;
            int expectedSampleCount = resolution * resolution;
            if (field.Count != expectedSampleCount)
                throw new ArgumentException(
                    $"Field sample count ({field.Count}) does not match observation grid ({expectedSampleCount}).",
                    nameof(field));

            TimeSpan frameDelay = TimeSpan.FromMilliseconds(1000.0 / _options.Fps);

            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();
            Console.WriteLine("2D console preview. Press Q or Esc to quit.");
            Console.WriteLine($"x=theta[{observation.StartIndex}], y=theta[{observation.StartIndex + 1}], RGB=current angles, scheme={_options.Render.ColorScheme}");

            try
            {
                while (!ShouldQuit())
                {
                    long started = Environment.TickCount64;
                    field.Step(_options.Render.TimeStep, _options.Render.SimulationStepsPerFrame);
                    Render(field, observation);

                    int delay = (int)(frameDelay.TotalMilliseconds - (Environment.TickCount64 - started));
                    if (delay > 0)
                        Thread.Sleep(delay);
                }
            }
            finally
            {
                Console.Write("\x1b[0m");
                Console.CursorVisible = true;
            }
        }

        void Render(PendulumSystemField field, ThetaObservation observation)
        {
            int resolution = observation.Resolution;
            var output = new StringBuilder(resolution * resolution * 20);
            output.Append("\x1b[3;1H");

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    RgbColor color = PendulumStateColorMap.Map(field[y * resolution + x], _options.Render.ColorScheme);
                    int brightness = (color.R + color.G + color.B) / 3;
                    char symbol = Symbols[brightness * (Symbols.Length - 1) / 255];

                    if (_options.Render.ColorScheme == PendulumColorScheme.RgbAngles)
                        output.Append($"\x1b[38;2;{color.R};{color.G};{color.B}m{symbol}{symbol}");
                    else
                        output.Append($"\x1b[38;2;{brightness};{brightness};{brightness}m{symbol}{symbol}");
                }

                output.Append("\x1b[0m");
                output.AppendLine();
            }

            Console.Write(output.ToString());
        }

        static bool ShouldQuit()
        {
            if (!Console.KeyAvailable)
                return false;

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            return key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape;
        }
    }
}
