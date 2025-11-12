// SwissLohnSystem.UI/Extensions/FormatExtensions.cs
using System.Globalization;

namespace SwissLohnSystem.UI.Extensions
{
    public static class FormatExtensions
    {
        public static string Chf(this decimal v) => v.ToString("C2", new CultureInfo("de-CH"));
        public static string Chf(this decimal? v) => v.HasValue ? v.Value.ToString("C2", new CultureInfo("de-CH")) : "—";
    }
}
