#if DEBUG
_ = Comanche.Session.Route(new[] {
    "gen",
    "even",
    "-s REPLACE_ME",
    "-t 3"
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