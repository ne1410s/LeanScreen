using Av;
using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Comanche;
using Crypt.IO;
using Crypt.Streams;

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

        IRenderingService renderer;
        if (fi.IsSecure())
        {
            var key = (keyCsv ?? "").Split(',').Select(b => byte.Parse(b)).ToArray();
            renderer = new FfmpegRenderer(new CryptoBlockReadStream(fi, key));
        }
        else if (fi.Exists) // this block is not necssary; but allows testing behaviour of block vs cryptoblock
        {
            renderer = new FfmpegRenderer(new BlockReadStream(fi));
        }
        else
        {
            renderer = new FfmpegRenderer(source);
        }

        var di = new DirectoryInfo(destination ?? Directory.GetCurrentDirectory());
        var snapper = new ThumbnailGenerator(renderer);
        var imager = new SixLaborsImagingService();
        var frameNumberFormat = renderer.TotalFrames.GetUpperBoundFormat();
        var itemNumberFormat = itemCount.GetUpperBoundFormat();
        var onFrameReceived = (RenderedFrame frame, int index) =>
        {
            using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
            var frameNo = frame.FrameNumber.ToString(frameNumberFormat);
            var itemNo = (index + 1).ToString(itemNumberFormat);
            var path = Path.Combine(di.FullName, $"{fi.Name}_item_{itemNo}_of_{itemCount}_frame_{frameNo}.jpg");
            File.WriteAllBytes(path, memStr.ToArray());
        };

        snapper.Generate(onFrameReceived, itemCount);
        return di.FullName;
    }
}
