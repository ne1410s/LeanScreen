using System.IO;

namespace Av.Services
{
    /// <summary>
    /// Provides file names.
    /// </summary>
    public interface ISingleFilePathProvider
    {
        /// <summary>
        /// Provides a file name, optionally constructed from source objects.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="count">The total count of items.</param>
        /// <returns>A path.</returns>
        string GetPath(FileInfo source, int count);
    }
}
