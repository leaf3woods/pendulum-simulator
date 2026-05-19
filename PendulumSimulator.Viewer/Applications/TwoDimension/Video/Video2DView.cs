using System.Diagnostics;
using ComputeSharp;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using PendulumSimulator.Analysis.Observation;
using PendulumSimulator.Core.GpuShader;
using PendulumSimulator.Core.PhysicsSystem;
using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Video
{
    /// <summary>
    /// 将二维初始角度扫描场渲染为视频；每个像素对应一个独立的摆系统样本。
    /// </summary>
    public class Video2DView : IPendulum2DView
    {
        private readonly Video2DViewOptions _options;
        private string _fileName;

        public Video2DView(Video2DViewOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ValidateOptions(options);
            _options = options;
            _fileName = "pendulum_2d.mp4";
        }

        public void Run(PendulumSystemField field, ThetaObservation observation)
        {
            ArgumentNullException.ThrowIfNull(field);
            ArgumentNullException.ThrowIfNull(observation);

            // 视频视图把二维观测网格直接映射到图像平面，因此只接受 2D observation。
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

            _fileName = $"pendulum_{observation.StartIndex + 1}x{field.PendulumCount}_{resolution}x{resolution}_{_options.Fps}fps.mp4";
            if(!Directory.Exists(_options.OutputDirectory))
            {
                Directory.CreateDirectory(_options.OutputDirectory);
            }
            var filePath = Path.Combine(_options.OutputDirectory, _fileName);

            // FFmpeg rawvideo 输入使用 BGR24，每个网格样本占 3 个字节。
            var frameBuffer = new byte[resolution * resolution * 3];
            var stopwatch = Stopwatch.StartNew();

            var totalBytes = (long)frameBuffer.Length * _options.FrameCount;
            var sizeMiB = totalBytes / (1024.0 * 1024.0);
            var encoder = ResolveVideoEncoder(_options);
            PrintHeader(observation, field, encoder, sizeMiB);

            var source = new PendulumFramePipeSource(field, frameBuffer, resolution, _options, stopwatch);
            var encoded = FFMpegArguments
                .FromPipeInput(source)
                .OutputToFile(filePath, overwrite: true, options =>
                {
                    options
                        .WithVideoCodec(encoder.Codec)
                        .WithFramerate(_options.Fps)
                        .ForcePixelFormat(encoder.PixelFormat)
                        .ForceFormat("mp4")
                        .WithFastStart();

                    if (encoder.ConstantRateFactor is int constantRateFactor)
                        options.WithConstantRateFactor(constantRateFactor);
                    if (encoder.SpeedPreset is Speed speedPreset)
                        options.WithSpeedPreset(speedPreset);

                    foreach (var argument in encoder.CustomArguments)
                        options.WithCustomArgument(argument);
                })
                .WithLogLevel(FFMpegLogLevel.Error)
                .ProcessSynchronously();

            if (!encoded)
                throw new InvalidOperationException($"FFmpeg could not encode {filePath}.");

            Console.WriteLine();
            Console.WriteLine($"done: {filePath}, {stopwatch.Elapsed.TotalSeconds:F2}s");
        }

        void PrintHeader(ThetaObservation observation, PendulumSystemField field, VideoEncoderPlan encoder, double size = 0)
        {
            var rows = new[]
            {
                (Item: "output",     Value: _fileName, Extra: "size",     Info: size == 0 ? "unknown" : $"{size / 12:0.00} MB"),
                (Item: "resolution", Value: $"[{observation.Resolution}]px x [{observation.Resolution}]px", Extra: "fps", Info: $"{_options.Fps} fps"),
                (Item: "frames",     Value: $"{_options.FrameCount} frames", Extra: "duration", Info: $"{_options.DurationSeconds} s"),
                (Item: "codec",      Value: encoder.Codec, Extra: "mode", Info: encoder.Description),
                (Item: "quality",    Value: encoder.Quality, Extra: "pixel", Info: encoder.PixelFormat),
                (Item: "observed",   Value: $"theta[{observation.StartIndex}], theta[{observation.StartIndex + 1}]", Extra: "systems", Info: $"{field.Count} pic")
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
            const int barWidth = 50;
            char[] heads = ['-', '\\', '|', '/'];
            double ratio = total == 0 ? 1.0 : (double)current / total;
            var filled = (int)Math.Round(ratio * barWidth);
            var head = (filled < barWidth) ? 1 : 0;
            var bar =
                new string('#', filled <= head ? 0 : filled - head)
                + (head == 1 ? $"{heads[current % heads.Length]}" : string.Empty)
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

        static void WriteBgrFrameGpu(
            byte[] buffer,
            int[] packedBuffer,
            ReadWriteBuffer<int> gpuFrame,
            PendulumFieldGpuRunner gpuField,
            PendulumColorScheme scheme)
        {
            var (dispatchWidth, dispatchHeight) = gpuField.DispatchSize;
            int grayscale = scheme == PendulumColorScheme.GrayscaleAngles ? 1 : 0;

            gpuField.Device.For(
                dispatchWidth,
                dispatchHeight,
                new PendulumBgrFrameShader(
                    gpuField.States,
                    gpuFrame,
                    gpuField.Count,
                    gpuField.StateStride,
                    grayscale));
            gpuFrame.CopyTo(packedBuffer);
            UnpackBgrFrame(packedBuffer, buffer);
        }

        static void WriteBgrFrameCpu(byte[] buffer, PendulumSystemField field, PendulumColorScheme scheme)
        {
            // Field 的线性布局与二维图像的 x + y * resolution 顺序一致，可直接顺序写入。
            for (int i = 0; i < field.Count; i++)
            {
                RgbColor color = PendulumStateColorMap.Map(field[i], scheme);
                var offset = i * 3;
                buffer[offset + 0] = color.B;
                buffer[offset + 1] = color.G;
                buffer[offset + 2] = color.R;
            }
        }

        static void UnpackBgrFrame(IReadOnlyList<int> packedBuffer, byte[] buffer)
        {
            for (int i = 0; i < packedBuffer.Count; i++)
            {
                int color = packedBuffer[i];
                int offset = i * 3;
                buffer[offset + 0] = (byte)(color & 0xFF);
                buffer[offset + 1] = (byte)((color >> 8) & 0xFF);
                buffer[offset + 2] = (byte)((color >> 16) & 0xFF);
            }
        }

        static void ValidateOptions(Video2DViewOptions options)
        {
            if (options.Fps <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Fps must be greater than 0.");
            if (options.DurationSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "DurationSeconds must be greater than 0.");
            if (string.IsNullOrEmpty(options.OutputDirectory))
                throw new ArgumentException("OutputDirectory cannot be empty.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.Codec))
                throw new ArgumentException("Codec cannot be empty.", nameof(options));
            if (options.ConstantRateFactor is < 0 or > 51)
                throw new ArgumentOutOfRangeException(nameof(options), "ConstantRateFactor must be between 0 and 51.");
            if (string.IsNullOrWhiteSpace(options.PixelFormat))
                throw new ArgumentException("PixelFormat cannot be empty.", nameof(options));
        }

        static VideoEncoderPlan ResolveVideoEncoder(Video2DViewOptions options)
        {
            if (!IsAuto(options.Codec))
                return CreateManualEncoderPlan(options);

            foreach (var candidate in HardwareEncoderCandidates)
            {
                var plan = candidate.ToPlan(options.ConstantRateFactor, options.PixelFormat);
                if (CanUseEncoder(plan))
                    return plan;
            }

            return new VideoEncoderPlan(
                Codec: "libx264",
                PixelFormat: ResolvePixelFormat(options.PixelFormat, "yuv420p"),
                Quality: $"crf {options.ConstantRateFactor}",
                Description: "CPU fallback",
                ConstantRateFactor: options.ConstantRateFactor,
                SpeedPreset: options.SpeedPreset,
                CustomArguments: []);
        }

        static VideoEncoderPlan CreateManualEncoderPlan(Video2DViewOptions options)
        {
            var hardwareCandidate = HardwareEncoderCandidates.FirstOrDefault(candidate =>
                candidate.Codec.Equals(options.Codec, StringComparison.OrdinalIgnoreCase));

            return hardwareCandidate is not null
                ? hardwareCandidate.ToPlan(options.ConstantRateFactor, options.PixelFormat)
                : new VideoEncoderPlan(
                    Codec: options.Codec,
                    PixelFormat: ResolvePixelFormat(options.PixelFormat, "yuv420p"),
                    Quality: $"crf {options.ConstantRateFactor}",
                    Description: "manual",
                    ConstantRateFactor: options.ConstantRateFactor,
                    SpeedPreset: options.SpeedPreset,
                    CustomArguments: []);
        }

        static bool CanUseEncoder(VideoEncoderPlan encoder)
        {
            var arguments =
                "-hide_banner -loglevel error " +
                "-f lavfi -i color=c=black:s=64x64:d=0.05 " +
                "-frames:v 1 -an " +
                $"-c:v {encoder.Codec} " +
                $"-pix_fmt {encoder.PixelFormat} " +
                string.Join(' ', encoder.CustomArguments) +
                " -f null -";

            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is null)
                    return false;

                if (!process.WaitForExit(milliseconds: 3000))
                {
                    process.Kill(entireProcessTree: true);
                    return false;
                }

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        static bool IsAuto(string value)
        {
            return value.Equals("auto", StringComparison.OrdinalIgnoreCase);
        }

        static string ResolvePixelFormat(string configuredValue, string fallback)
        {
            return IsAuto(configuredValue) ? fallback : configuredValue;
        }

        static int ClampQuality(int quality)
        {
            return Math.Clamp(quality, 0, 51);
        }

        sealed class PendulumFramePipeSource(
            PendulumSystemField field,
            byte[] frameBuffer,
            int resolution,
            Video2DViewOptions options,
            Stopwatch stopwatch) : IPipeSource
        {
            const string PixelFormat = "bgr24";

            public string GetStreamArguments()
                => $"-f rawvideo -r {options.Fps} -pix_fmt {PixelFormat} -s {resolution}x{resolution}";

            public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
            {
                if (PendulumFieldGpuRunner.IsSupportedPendulumCount(field.PendulumCount))
                {
                    try
                    {
                        await WriteGpuFramesAsync(outputStream, cancellationToken);
                        return;
                    }
                    catch (GpuRenderUnavailableException)
                    {
                        Console.WriteLine("GPU render path unavailable; falling back to CPU.");
                    }
                }

                await WriteCpuFramesAsync(outputStream, cancellationToken);
            }

            async Task WriteGpuFramesAsync(Stream outputStream, CancellationToken cancellationToken)
            {
                using var gpuField = CreateGpuRunner(field);
                using var gpuFrame = CreateGpuFrame(gpuField, field.Count);
                var packedFrameBuffer = new int[field.Count];
                var framesWritten = 0;

                for (int frame = 0; frame < options.FrameCount; frame++)
                {
                    try
                    {
                        // 状态和颜色映射都留在 GPU，只把最终 BGR 帧拷回 CPU 交给 FFmpeg。
                        gpuField.Step((float)options.Render.TimeStep, options.Render.SimulationStepsPerFrame);
                        WriteBgrFrameGpu(frameBuffer, packedFrameBuffer, gpuFrame, gpuField, options.Render.ColorScheme);
                    }
                    catch (Exception exception) when (framesWritten == 0)
                    {
                        throw new GpuRenderUnavailableException(exception);
                    }

                    await WriteFrameAsync(outputStream, cancellationToken);
                    framesWritten++;
                    WriteProgress(frame + 1, options.FrameCount, stopwatch.Elapsed);
                }
            }

            async Task WriteCpuFramesAsync(Stream outputStream, CancellationToken cancellationToken)
            {
                for (int frame = 0; frame < options.FrameCount; frame++)
                {
                    field.Step(options.Render.TimeStep, options.Render.SimulationStepsPerFrame, useGpu: false);
                    WriteBgrFrameCpu(frameBuffer, field, options.Render.ColorScheme);

                    await WriteFrameAsync(outputStream, cancellationToken);
                    WriteProgress(frame + 1, options.FrameCount, stopwatch.Elapsed);
                }
            }

            Task WriteFrameAsync(Stream outputStream, CancellationToken cancellationToken)
                => outputStream.WriteAsync(frameBuffer, 0, frameBuffer.Length, cancellationToken);

            static PendulumFieldGpuRunner CreateGpuRunner(PendulumSystemField field)
            {
                try
                {
                    return new PendulumFieldGpuRunner(field);
                }
                catch (Exception exception)
                {
                    throw new GpuRenderUnavailableException(exception);
                }
            }

            static ReadWriteBuffer<int> CreateGpuFrame(PendulumFieldGpuRunner gpuField, int count)
            {
                try
                {
                    return gpuField.Device.AllocateReadWriteBuffer<int>(count);
                }
                catch (Exception exception)
                {
                    throw new GpuRenderUnavailableException(exception);
                }
            }
        }

        sealed class GpuRenderUnavailableException(Exception innerException) : Exception(
            "GPU render path is unavailable.",
            innerException)
        {
        }

        sealed record VideoEncoderPlan(
            string Codec,
            string PixelFormat,
            string Quality,
            string Description,
            int? ConstantRateFactor,
            Speed? SpeedPreset,
            string[] CustomArguments);

        sealed record HardwareEncoderCandidate(
            string Codec,
            string Description,
            string PixelFormat,
            string QualityName,
            Func<int, string[]> CreateArguments)
        {
            public VideoEncoderPlan ToPlan(int quality, string configuredPixelFormat)
            {
                var normalizedQuality = ClampQuality(quality);
                return new VideoEncoderPlan(
                    Codec,
                    ResolvePixelFormat(configuredPixelFormat, PixelFormat),
                    $"{QualityName} {normalizedQuality}",
                    Description,
                    ConstantRateFactor: null,
                    SpeedPreset: null,
                    CreateArguments(normalizedQuality));
            }
        }

        static readonly HardwareEncoderCandidate[] HardwareEncoderCandidates =
        [
            new(
                Codec: "hevc_nvenc",
                Description: "NVIDIA NVENC HEVC",
                PixelFormat: "yuv420p",
                QualityName: "cq",
                CreateArguments: quality =>
                [
                    "-preset medium",
                    "-rc vbr",
                    $"-cq {quality}",
                    "-b:v 0"
                ]),
            new(
                Codec: "h264_nvenc",
                Description: "NVIDIA NVENC H.264",
                PixelFormat: "yuv420p",
                QualityName: "cq",
                CreateArguments: quality =>
                [
                    "-preset medium",
                    "-rc vbr",
                    $"-cq {quality}",
                    "-b:v 0"
                ]),
            new(
                Codec: "hevc_qsv",
                Description: "Intel QSV HEVC",
                PixelFormat: "nv12",
                QualityName: "global_quality",
                CreateArguments: quality =>
                [
                    "-preset medium",
                    $"-global_quality {quality}"
                ]),
            new(
                Codec: "h264_qsv",
                Description: "Intel QSV H.264",
                PixelFormat: "nv12",
                QualityName: "global_quality",
                CreateArguments: quality =>
                [
                    "-preset medium",
                    $"-global_quality {quality}"
                ]),
            new(
                Codec: "hevc_amf",
                Description: "AMD AMF HEVC",
                PixelFormat: "yuv420p",
                QualityName: "qp",
                CreateArguments: quality =>
                [
                    "-quality balanced",
                    "-rc cqp",
                    $"-qp_i {quality}",
                    $"-qp_p {quality}",
                    $"-qp_b {Math.Min(quality + 2, 51)}"
                ]),
            new(
                Codec: "h264_amf",
                Description: "AMD AMF H.264",
                PixelFormat: "yuv420p",
                QualityName: "qp",
                CreateArguments: quality =>
                [
                    "-quality balanced",
                    "-rc cqp",
                    $"-qp_i {quality}",
                    $"-qp_p {quality}",
                    $"-qp_b {Math.Min(quality + 2, 51)}"
                ])
        ];
    }
}
