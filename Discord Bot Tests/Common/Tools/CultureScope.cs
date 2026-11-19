using System.Globalization;

namespace Discord_Bot.Tests.Common.Tools;

/// <summary>
/// Runs part of a test under another culture, such as hu-HU (the dev PC's), to check that the output doesn't depend on the
/// machine; disposing it restores the previous culture. The culture flows with the async context, so other tests don't see it.
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo previousCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo previousUICulture = CultureInfo.CurrentUICulture;

    public CultureScope(string name)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = previousCulture;
        CultureInfo.CurrentUICulture = previousUICulture;
    }
}
