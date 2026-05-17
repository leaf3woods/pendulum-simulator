using PendulumSimulator.Viewer.Rendering;

namespace PendulumSimulator.Viewer.Applications.TwoDimension.Video
{
    /// <summary>
    /// 视频输出设置；物理推进参数放在 <see cref="Render"/> 中，视频容器参数放在本类型中。
    /// </summary>
    public record Video2DViewOptions
    {
        public static Video2DViewOptions Default { get; } = new();

        /// <summary>
        /// 控制颜色映射以及每一帧之前要推进多少物理时间。
        /// </summary>
        public RenderOptions Render { get; init; } = new();

        public string OutputDirectory { get; init; } = string.Empty;

        public int Fps { get; init; } = 30;

        public int DurationSeconds { get; init; } = 20;

        public string Codec { get; init; } = "mp4v";

        /// <summary>
        /// 输出帧数由视频帧率和播放时长决定，与物理模拟时长不一定相同。
        /// </summary>
        public int FrameCount => Fps * DurationSeconds;
    }
}
