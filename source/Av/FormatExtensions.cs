namespace Av
{
    /// <summary>
    /// Extensions for formatting.
    /// </summary>
    public static class FormatExtensions
    {
        private const double Kilobyte = 1024;
        private const double Megabyte = 1048576;
        private const double Gigabyte = 1073741824;
        private const double Terabyte = 1099511627776;

        /// <summary>
        /// Formats a byte count as size on disk.
        /// </summary>
        /// <param name="bytes">The byte count.</param>
        /// <returns>Size on disk.</returns>
        public static string FormatSize(this long bytes) =>
            bytes < Kilobyte ? $"{bytes}b"
                : bytes < Megabyte ? $"{bytes / Kilobyte:N0}kb"
                : bytes < Gigabyte ? $"{bytes / Megabyte:N0}mb"
                : bytes < Terabyte ? $"{bytes / Gigabyte:N0}gb"
                : $"{bytes / Terabyte:N0}tb";
    }
}
