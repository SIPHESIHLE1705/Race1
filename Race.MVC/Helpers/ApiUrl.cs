usiusing System.Globalization;

namespace Race.MVC.Helpers
{
    // Builds URLs such as api/Events?eventName=City%20Run&eventDistance=10.5
    // (the Part 2 API takes its inputs from the query string)
    public static class ApiUrl
    {
        // Builds an API URL by adding the supplied values as query string parameters.
        public static string Build(string path, params (string Key, object? Value)[] query)
        {
            // Returns the original path if no query parameters were supplied.
            if (query.Length == 0) return path;

            // Converts each query parameter into an encoded key-value pair.
            // URI encoding ensures special characters and spaces are handled correctly.
            var parts = query.Select(q =>
                $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(Format(q.Value))}");

            // Joins all query parameters with '&' and adds them to the API path.
            return path + "?" + string.Join("&", parts);
        }

        // Converts different data types into strings before adding them to the URL.
        private static string Format(object? value) => value switch
        {
            // Converts null values into empty strings.
            null => string.Empty,

            // Returns string values without changing them.
            string s => s,

            // Formats dates in year-month-day format.
            DateTime d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),

            // Formats time values in a consistent format, for example 08:00:00.
            TimeSpan t => t.ToString("c", CultureInfo.InvariantCulture),

            // Formats other supported values using a culture-independent format.
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),

            // Converts any remaining values into strings.
            _ => value.ToString() ?? string.Empty
        };
    }
}
