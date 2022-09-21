using System.IO;

namespace Av.Services
{
    /// <inheritdoc cref="ISingleFilePathProvider"/>
    public class DefaultSingleFilePathProvider : ISingleFilePathProvider
    {
        /// <inheritdoc/>
        public string GetPath(FileInfo source, int count)
            => Path.Combine(source.DirectoryName, $"{source.Name}_x{count}.png");
    }
}
