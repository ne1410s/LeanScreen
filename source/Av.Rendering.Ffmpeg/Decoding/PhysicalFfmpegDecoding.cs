namespace Av.Rendering.Ffmpeg.Decoding
{
    /// <summary>
    /// Ffmpeg decoding session for physical sources.
    /// </summary>
    internal sealed unsafe class PhysicalFfmpegDecoding : FfmpegDecodingSessionBase
    {
        /// <summary>
        /// Initialises a new <see cref="PhysicalFfmpegDecoding"/>.
        /// </summary>
        /// <param name="url">The url to the physical media.</param>
        public PhysicalFfmpegDecoding(string url)
            : base(url)
        {
            OpenInputContext();
        }
    }
}
