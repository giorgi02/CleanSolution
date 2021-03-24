using System.Collections.Generic;
using System.Globalization;

namespace Workabroad.Presentation.Admin.Extensions.Services
{
    public enum Cultures : byte
    {
        Geo = 1,
        Eng = 2
    }
    public static class LocalizationExtensions
    {
        private static readonly Dictionary<string, byte> CultureMap = new Dictionary<string, byte>()
        {
            ["ka-GE"] = (byte)Cultures.Geo,
            ["en-US"] = (byte)Cultures.Eng
        };

        public static byte ToCultureID(this CultureInfo culture) =>
            CultureMap.TryGetValue(culture.Name, out var id) ? id : (byte)Cultures.Geo;
    }
}
