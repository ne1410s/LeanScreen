#if DEBUG
_ = Comanche.Session.Route(new[] {
    "demo",
    "add",
    "--n1 1",
    "--n2 2"
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