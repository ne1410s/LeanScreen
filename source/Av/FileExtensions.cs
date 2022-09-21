using System.IO;
using Av.Models;
using Crypt.IO;

namespace Av
{
    /// <summary>
    /// Extensions for <see cref="FileInfo"/>.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Gets media type information for the file.
        /// </summary>
        /// <param name="fi">The file.</param>
        /// <returns>Media type information.</returns>
        public static MediaTypeInfo GetMediaTypeInfo(this FileInfo fi)
            => MediaTypeInfo.Get(fi.Extension);
    }
}
