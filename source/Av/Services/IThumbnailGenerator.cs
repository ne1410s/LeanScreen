namespace Av.Services
{
    /// <summary>
    /// Generates thumbnails.
    /// </summary>
    public interface IThumbnailGenerator
    {
        /// <summary>
        /// Dumps evenly-distributed thumbnails to individual files.
        /// </summary>
        /// <param name="filePath">The source path.</param>
        /// <param name="count">The total number of thumbs.</param>
        /// <param name="maxWidth">The maximum width of each file.</param>
        /// <param name="pathProvider">The path provider. By default, files are
        /// generated alongside the source file.</param>
        void DumpMany(
            string filePath,
            int count,
            int maxWidth,
            ISequencedFilePathProvider pathProvider = null);

        /// <summary>
        /// Dumps evenly-distributed thumbnails to a single file.
        /// </summary>
        /// <param name="filePath">The source path.</param>
        /// <param name="count">The total number of thumbs.</param>
        /// <param name="maxWidth">The maximum width of each item.</param>
        /// <param name="pathProvider">The path provider. By default, a file is
        /// generated alongside the source file.</param>
        void DumpCollation(
            string filePath,
            int count,
            int maxWidth,
            ISingleFilePathProvider pathProvider = null);
    }
}
