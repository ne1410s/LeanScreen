using Comanche;

namespace AvCtl;

/// <summary>
/// A demo module.
/// </summary>
[Alias("demo")]
public static class DemoModule
{
    /// <summary>
    /// A demo function.
    /// </summary>
    /// <param name="n1">Number one.</param>
    /// <param name="n2">Number two.</param>
    /// <returns>The sum.</returns>
    public static int Add(int n1, int n2) => n1 + n2;
}
