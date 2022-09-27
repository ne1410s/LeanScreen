using Av.Abstractions.Rendering;
using Av.Imaging.SixLabors;
using Av.Rendering.Ffmpeg;
using Av.Services;
using Comanche;

namespace AvCtl;

/// <summary>
/// Generate module.
/// </summary>
[Alias("gen")]
public static class GenerateModule
{
    /// <summary>
    /// Generates frames from a video source.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="itemCount">The total number of items.</param>
    public static void Even(
        [Alias("s")]string source,
        [Alias("t")]int itemCount = 24)
    {
        //source = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4";
        //source = "C:\\Users\\Paul.Jones\\Videos\\sample.mp4";
        source = "C:\\temp\\media\\big_buck_bunny.mp4";

        var renderer = new FfmpegRenderer(source);
        var snapper = new ThumbnailGenerator(renderer);
        snapper.Generate(OnFrameReceived, itemCount);
    }

    private static void OnFrameReceived(RenderedFrame frame, int index)
    {
        var imager = new SixLaborsImagingService();
        using var memStr = imager.Encode(frame.Rgb24Bytes, frame.Dimensions);
        File.WriteAllBytes($"item-{index}_frame-{frame.FrameNumber:D8}.jpg", memStr.ToArray());
    }
}
