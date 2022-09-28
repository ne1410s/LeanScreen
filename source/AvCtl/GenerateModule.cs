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
    /// <returns>The output path.</returns>
    public static string Evenly(
        [Alias("s")]string source,
        [Alias("d")]string? destination = null,
        [Alias("t")]int itemCount = 24)
    {
        if (destination == null)
        {
            var fi = new FileInfo(source);
            destination = fi.DirectoryName ?? string.Empty;
        }

        Directory.CreateDirectory(destination);
        var renderer = new FfmpegRenderer(source);
        var snapper = new ThumbnailGenerator(renderer);
        var imager = new SixLaborsImagingService();

        var onFrameReceived = (RenderedFrame frame, int index) =>
        {
            using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
            var path = Path.Combine(destination, $"item-{index}_frame-{frame.FrameNumber:D8}.jpg");
            File.WriteAllBytes(path, memStr.ToArray());
        };

        snapper.Generate(onFrameReceived, itemCount);
        return new DirectoryInfo(destination).FullName;
    }
}
