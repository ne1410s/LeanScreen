using Av.Models;
using Av.Renderer.Ffmpeg;
using Av.Services;
using Comanche;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
        //source = "C:\\Users\\Paul.Jones\\Videos\\sample.avi";
        source = "C:\\temp\\media\\sample.mp4";

        var x = new FfmpegFrameRenderingService();
        var snapper = new ThumbnailGenerator(x);
        snapper.Generate(source, OnFrameReceived, itemCount: itemCount);
    }

    private static void OnFrameReceived(RenderedFrame frame, int index)
    {
        var dims = frame.Dimensions;
        var img = Image.LoadPixelData<Rgb24>(frame.Rgb24Bytes, dims.Width, dims.Height);
        img.Save($"item-{index}_frame-{frame.FrameNumber}.jpg");
    }
}
