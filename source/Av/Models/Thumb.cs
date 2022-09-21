using System.Drawing;

namespace Av.Models
{
    /// <summary>
    /// Thumbnail image, together with information.
    /// </summary>
    public class Thumb : ThumbInfo
    {
        /// <summary>
        /// Gets or sets the raw image bitmap.
        /// </summary>
        public Bitmap RawImage { get; set; }
    }
}
