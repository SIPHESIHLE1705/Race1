using System.Globalization;

namespace Race.MVC.Helpers
{
    // Builds URLs such as  api/Events?eventName=City%20Run&eventDistance=10.5
    // (the Part 2 API takes its inputs from the query string)
    public static class ApiUrl
    {
        public static string Build(string path, params (string Key, object? Value)[] query)
        {
            if (query.Length == 0) return path;

            var parts = query.Select(q =>
                $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(Format(q.Value))}");

            return path + "?" + string.Join("&", parts);
        }

        private static string Format(object? value) => value switch
        {
            null => string.Empty,
            string s => s,
            DateTime d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeSpan t => t.ToString("c", CultureInfo.InvariantCulture),   // 08:00:00
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
