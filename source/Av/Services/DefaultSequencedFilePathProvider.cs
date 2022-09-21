using System.IO;

namespace Av.Services
{
    /// <inheritdoc cref="ISequencedFilePathProvider"/>
    public class DefaultSequencedFilePathProvider : ISequencedFilePathProvider
    {
        /// <inheritdoc/>
        public string GetPath(FileInfo source, int number, int count)
            => Path.Combine(source.DirectoryName, $"{source.Name}_{number}_of_{count}.png");
    }
}
