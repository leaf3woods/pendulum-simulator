using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenCvSharp;
using PendulumSimulator.Analysis;
using PendulumSimulator.Core.PhysicsSystem;
using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Video
{
    public class Video2DView : IPendulum2DView
    {
        private readonly Video2DViewOptions _options;

        public Video2DView(Video2DViewOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ValidateOptions(options);
            _options = options;
        }

        public void Run(PendulumSystemField field, ThetaObservation observation)
        {
            ArgumentNullException.ThrowIfNull(field);
            ArgumentNullException.ThrowIfNull(observation);

            if (observation.Dimension != 2)
                throw new ArgumentException(
                    $"Video2DView requires a 2D observation; got Dimension={observation.Dimension}.",
                    nameof(observation));

            var resolution = observation.Resolution;
            var expectedSampleCount = resolution * resolution;
            if (field.Count != expectedSampleCount)
                throw new ArgumentException(
                    $"Field sample count ({field.Count}) does not match observation grid ({expectedSampleCount}).",
                    nameof(field));

            var frameBuffer = new byte[resolution * resolution * 3];

            using var writer = new VideoWriter(
                _options.OutputPath,
                VideoWriter.FourCC(_options.Codec[0], _options.Codec[1], _options.Codec[2], _options.Codec[3]),
                _options.Fps,
                new Size(resolution, resolution));

            if (!writer.IsOpened())
                throw new InvalidOperationException($"Video writer could not open {_options.OutputPath}.");

            using var frameMat = new Mat(resolution, resolution, MatType.CV_8UC3);
            var stopwatch = Stopwatch.StartNew();

            var totalBytes = (long)frameBuffer.Length * _options.FrameCount;
            var sizeMiB = totalBytes / (1024.0 * 1024.0);
            PrintHeader(observation, field, sizeMiB);

            for (int frame = 0; frame < _options.FrameCount; frame++)
            {
                field.Step(_options.Render.TimeStep, _options.Render.SimulationStepsPerFrame);
                WriteBgrFrame(frameBuffer, field, _options.Render.ColorScheme);
                Marshal.Copy(frameBuffer, 0, frameMat.Data, frameBuffer.Length);
                writer.Write(frameMat);

                WriteProgress(frame + 1, _options.FrameCount, stopwatch.Elapsed);
            }

            Console.WriteLine();
            Console.WriteLine($"done: {_options.OutputPath}, {stopwatch.Elapsed.TotalSeconds:F2}s");
        }

        void PrintHeader(ThetaObservation observation, PendulumSystemField field, double size = 0)
        {
            var rows = new[]
            {
                (Item: "output",     Value: _options.OutputPath, Extra: "size",     Info: size == 0 ? "unknown" : $"{size:0.00} MB"),
                (Item: "resolution", Value: $"({observation.Resolution} x {observation.Resolution})", Extra: "fps", Info: $"{_options.Fps} fps"),
                (Item: "frames",     Value: $"{_options.FrameCount} frames", Extra: "duration", Info: $"{_options.DurationSeconds} s"),
                (Item: "observed",   Value: $"theta[{observation.StartPendulumIndex}], theta[{observation.StartPendulumIndex + 1}]", Extra: "systems", Info: $"{field.Count} pic")
            };

            var headers = new[] { "item", "value", "extra", "info" };

            var widths = new[]
            {
                Math.Max(headers[0].Length, rows.Max(x => x.Item.Length)),
                Math.Max(headers[1].Length, rows.Max(x => x.Value.Length)),
                Math.Max(headers[2].Length, rows.Max(x => x.Extra.Length)),
                Math.Max(headers[3].Length, rows.Max(x => x.Info.Length))
            };

            var format = $"{{0,-{widths[0]}}} | {{1,-{widths[1]}}} | {{2,-{widths[2]}}} | {{3,-{widths[3]}}}";

            Console.WriteLine(format, headers[0], headers[1], headers[2], headers[3]);
            Console.WriteLine(string.Join("-+-", widths.Select(w => new string('-', w))));

            foreach (var (Item, Value, Extra, Info) in rows)
            {
                Console.WriteLine(format, Item, Value, Extra, Info);
            }
        }

        static void WriteProgress(int current, int total, TimeSpan elapsed)
        {
            const int barWidth = 30;
            double ratio = total == 0 ? 1.0 : (double)current / total;
            var filled = (int)Math.Round(ratio * barWidth);
            var head = (filled > 0 && filled < barWidth) ? 1 : 0;
            var bar =
                new string('=', filled - head)
                + (head == 1 ? ">" : string.Empty)
                + new string(' ', barWidth - filled);

            var etaSeconds = current > 0
                ? elapsed.TotalSeconds * (total - current) / current
                : 0.0;

            var etaDisplay = etaSeconds <= 60
                ? $"{etaSeconds:F1}s"
                : TimeSpan.FromSeconds(etaSeconds).ToString(@"hh\:mm\:ss");
            var output = $"\r[{bar}] {ratio * 100,5:F1}% ({current}/{total}) elapsed " + etaDisplay;
            Console.Write(output);
        }

        static void WriteBgrFrame(byte[] buffer, PendulumSystemField field, PendulumColorScheme scheme)
        {
            for (int i = 0; i < field.Count; i++)
            {
                RgbColor color = PendulumStateColorMap.Map(field[i], scheme);
                var offset = i * 3;
                buffer[offset + 0] = color.B;
                buffer[offset + 1] = color.G;
                buffer[offset + 2] = color.R;
            }
        }

        static void ValidateOptions(Video2DViewOptions options)
        {
            if (options.Fps <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Fps must be greater than 0.");
            if (options.DurationSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "DurationSeconds must be greater than 0.");
            if (string.IsNullOrEmpty(options.OutputPath))
                throw new ArgumentException("OutputPath cannot be empty.", nameof(options));
            if (options.Codec is null || options.Codec.Length != 4)
                throw new ArgumentException("Codec must contain exactly 4 characters.", nameof(options));
        }
    }
}
