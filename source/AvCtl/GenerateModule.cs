using Av;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Comanche;

namespace AvCtl;

/// <summary>
/// Generate module.
/// </summary>
[Alias("snap")]
public static class SnapshotModule
{
    /// <summary>
    /// Generates frames from a video source.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="destination">The output folder.</param>
    /// <param name="itemCount">The total number of items.</param>
    /// <param name="keyCsv">Key (if source is encrypted).</param>
    /// <returns>The output path.</returns>
    public static string Evenly(
        [Alias("s")]string source,
        [Alias("d")]string? destination = null,
        [Alias("t")]int itemCount = 24,
        [Alias("k")]string? keyCsv = null)
    {
        var fi = new FileInfo(source);
        if (destination == null && fi.Exists)
        {
            destination = fi.DirectoryName;
        }

        var key = (keyCsv ?? "").Split(',').Select(b => byte.Parse(b)).ToArray();
        IRenderingService renderer = new FfmpegRenderer(source, key);
        var di = new DirectoryInfo(destination ?? Directory.GetCurrentDirectory());
        var snapper = new ThumbnailGenerator(renderer);
        var imager = new SixLaborsImagingService();
        var onFrameReceived = (RenderedFrame frame, int index) =>
        {
            using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
            var itemNo = (index + 1L).FormatToUpperBound(itemCount);
            var frameNo = frame.FrameNumber.FormatToUpperBound(renderer.TotalFrames);
            var path = Path.Combine(di.FullName, $"n{itemNo}_f{frameNo}.jpg");
            File.WriteAllBytes(path, memStr.ToArray());
        };

        snapper.Generate(onFrameReceived, itemCount);
        return di.FullName;
    }
}
