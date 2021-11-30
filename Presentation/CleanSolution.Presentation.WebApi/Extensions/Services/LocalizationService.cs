using System.Globalization;

namespace CleanSolution.Presentation.WebApi.Extensions.Services;
public enum Cultures : byte
{
    Geo = 1,
    Eng = 2
}
public static class LocalizationService
{
    private static readonly Dictionary<string, byte> CultureMap = new()
    {
        ["ka-GE"] = (byte)Cultures.Geo,
        ["en-US"] = (byte)Cultures.Eng
    };

    public static byte ToCultureID(this CultureInfo culture) =>
        CultureMap.TryGetValue(culture.Name, out var id) ? id : (byte)Cultures.Geo;
}