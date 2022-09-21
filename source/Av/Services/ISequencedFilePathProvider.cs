using System.IO;

namespace Av.Services
{
    /// <summary>
    /// Provides file names in a sequential context.
    /// </summary>
    public interface ISequencedFilePathProvider
    {
        /// <summary>
        /// Provides a file name, optionally constructed from source objects.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="number">The number in the sequence.</param>
        /// <param name="count">The total count of files.</param>
        /// <returns>A path.</returns>
        string GetPath(FileInfo source, int number, int count);
    }
}
