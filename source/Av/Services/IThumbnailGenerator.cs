namespace Av.Services
{
    /// <summary>
    /// Generates thumbnails.
    /// </summary>
    public interface IThumbnailGenerator
    {
        /// <summary>
        /// Dumps evenly-distributes thumbnails to file.
        /// </summary>
        /// <returns>Thumb list.</returns>
        void DumpEvenly(string filePath, int count);
    }
}
