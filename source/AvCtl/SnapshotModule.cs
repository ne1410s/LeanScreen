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
    /// <param name="total">The total number of thumbs.</param>
    /// <param name="maxWidth">The maximum thumb width.</param>
    /// <param name="discrete">If true, generates individual files, rather than
    /// the default single collated file.</param>
    public static void Evenly(
        [Alias("s")]string source,
        [Alias("t")]int total = 24,
        [Alias("w")]int? maxWidth = null,
        [Alias("d")]bool discrete = false)
    {
        var sut = new ThumbnailGenerator();
        if (discrete) sut.DumpMany(source, total, maxWidth ?? 400);
        else sut.DumpCollation(source, total, maxWidth ?? 200);
    }
}
