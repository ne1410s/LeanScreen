#if DEBUG
_ = Comanche.Session.Route(new[] {
    "snap",
    "evenly",
    "-s c:\\temp\\media\\sample.mp4",
    "-t 3",
    "-d"
});
#else
_ = Comanche.Session.Route();
#endif

/// <summary>
/// The program.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#pragma warning disable CA1050
public partial class Program { }
#pragma warning restore CA1050