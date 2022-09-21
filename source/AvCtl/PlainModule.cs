using Av.Services;
using Comanche;

namespace AvCtl;

/// <summary>
/// Snapshots module.
/// </summary>
[Alias("snap")]
public static class SnapshotModule
{
    /// <summary>
    /// Dumps snapshots of the source.
    /// </summary>
    /// <param name="source">The source file.</param>
    /// <param name="count">The number of thumbs.</param>
    public static void Evenly([Alias("s")]string source, [Alias("c")]int count = 24)
    {
        var sut = new ThumbnailGenerator();
        sut.DumpEvenly(source, count);
    }
}
