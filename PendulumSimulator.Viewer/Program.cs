using Microsoft.Extensions.Configuration;
using PendulumSimulator.Analysis;
using PendulumSimulator.Viewer.Applications.ThreeDimension;
using PendulumSimulator.Viewer.Applications.TwoDimension.Realtime;
using PendulumSimulator.Viewer.Applications.TwoDimension.Video;
using PendulumSimulator.Viewer.Options;
using PendulumSimulator.Viewer.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PendulumSimulator.Viewer
{
    internal static class Program
    {
        public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            Converters =
            {
                new JsonNumberEnumConverter<ViewerMode>(),
                new JsonNumberEnumConverter<PendulumColorScheme>(),
            },
        };
        private static void Main(string[] args)
        {
            ViewerMode mode;
            var command = ViewerCommand.Parse(args);
            var baseConfiguration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var baseOptions = baseConfiguration.Get<SimulatorConfiguration>();
            if(baseOptions is null)
            {
                baseOptions = SimulatorConfiguration.Example;
                var json = JsonSerializer.Serialize(SimulatorConfiguration.Example, SerializerOptions);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), json);
            }
            mode = command?.Mode ?? baseOptions?.Mode ??  ViewerMode.Video;

            var modeConfigFile = mode switch
            {
                ViewerMode.Console => "appsettings.console.json",
                ViewerMode.ThreeDimensional => "appsettings.3d.json",
                ViewerMode.Video => "appsettings.video.json",
                _ => throw new ArgumentException($"Unsupported viewer mode: {mode}")
            };
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile(modeConfigFile, optional: true, reloadOnChange: false)
                .Build();

            var options = configuration.Get<SimulatorConfiguration>()
                ?? SimulatorConfiguration.Example;
            options.ColorScheme = command?.ColorScheme ?? options?.ColorScheme ?? PendulumColorScheme.RgbAngles;

            switch (mode)
            {
                case ViewerMode.Console:
                    RunConsole(options);
                    break;

                case ViewerMode.ThreeDimensional:
                    RunBuilder3D(options);
                    break;

                case ViewerMode.Video:
                    RunVideo(options);
                    break;

                default:
                    throw new ArgumentException($"Unsupported viewer mode: {mode}");
            }
        }

        static void RunVideo(SimulatorConfiguration? config = null)
        {
            var spec = config?.SystemSpec is not null ?
                PendulumSystemSpec.Uniform(config.SystemSpec.PendulumCount, config.SystemSpec.Mass, config.SystemSpec.Length) :
                PendulumSystemSpec.Default;
            var observation = config?.Observation is not null ?
                ThetaObservation.Default with
                {
                    StartPendulumIndex = (config?.Observation?.StartPendulumIndex)!.Value,
                    Dimension = (config?.Observation?.Dimension)!.Value,
                    Resolution = (config?.Observation?.Resolution)!.Value,
                } :
                ThetaObservation.Default with { Resolution = 512 };
            var field = PendulumSystemFieldFactory.Build(spec, observation);

            var render = config?.Render is not null ?
                Video2DViewOptions.Default.Render with
                {
                    ColorScheme = config.ColorScheme!.Value,
                    TimeStep = config.Render.TimeStep,
                    SimulationStepsPerFrame = config.Render.SimulationStepsPerFrame
                } :
                Video2DViewOptions.Default.Render with { ColorScheme = config!.ColorScheme!.Value };

            var options = Video2DViewOptions.Default with
            {
                Render = render,
                DurationSeconds = config.DurationSeconds,
                Fps = config.Fps,
            };
            var view = new Video2DView(options);
            view.Run(field, observation);
        }

        static void RunConsole(SimulatorConfiguration? config = null)
        {
            var spec = config?.SystemSpec is not null ?
                PendulumSystemSpec.Uniform(config.SystemSpec.PendulumCount, config.SystemSpec.Mass, config.SystemSpec.Length) :
                PendulumSystemSpec.Default;
            var observation = config?.Observation is not null ?
                ThetaObservation.Default with
                {
                    StartPendulumIndex = (config?.Observation?.StartPendulumIndex)!.Value,
                    Dimension = (config?.Observation?.Dimension)!.Value,
                    Resolution = (config?.Observation?.Resolution)!.Value,
                } :
                ThetaObservation.Default with { Resolution = 512 };
            var field = PendulumSystemFieldFactory.Build(spec, observation);

            var render = config?.Render is not null ?
                Console2DViewOptions.Default.Render with
                {
                    ColorScheme = config.ColorScheme!.Value,
                    TimeStep = config.Render.TimeStep,
                    SimulationStepsPerFrame = config.Render.SimulationStepsPerFrame
                } :
                Console2DViewOptions.Default.Render with { ColorScheme = config!.ColorScheme!.Value };

            var options = Console2DViewOptions.Default with
            {
                Render = render,
            };
            var view = new Console2DView(options);
            view.Run(field, observation);
        }

        static void RunBuilder3D(SimulatorConfiguration? config = null)
        {
            var spec = config?.SystemSpec is not null ?
                PendulumSystemSpec.Uniform(config.SystemSpec.PendulumCount, config.SystemSpec.Mass, config.SystemSpec.Length) :
                PendulumSystemSpec.Default;
            var observation = config?.Observation is not null ?
                ThetaObservation.Default with
                {
                    StartPendulumIndex = (config?.Observation?.StartPendulumIndex)!.Value,
                    Dimension = (config?.Observation?.Dimension)!.Value,
                    Resolution = (config?.Observation?.Resolution)!.Value,
                } :
                ThetaObservation.Default with { Resolution = 512 };
            var field = PendulumSystemFieldFactory.Build(spec, observation);

            var render = config?.Render is not null ?
                Builder3DViewOptions.Default.Render with
                {
                    ColorScheme = config!.ColorScheme!.Value,
                    TimeStep = config.Render.TimeStep,
                    SimulationStepsPerFrame = config.Render.SimulationStepsPerFrame
                } :
                Builder3DViewOptions.Default.Render with { ColorScheme = config!.ColorScheme!.Value };

            var options = Builder3DViewOptions.Default with
            {
                Render = render,
            };
            var view = new Builder3DView(options);
            view.Run(field, observation);
        }
    }
}
