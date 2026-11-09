namespace Race.MVC.Helpers
{
    // Small display helpers shared by the views
    public static class EventUi
    {
        // Returns the Bootstrap icon class based on the event type.
        public static string Icon(string eventType) => eventType switch
        {
            "Run" => "bi-lightning-charge-fill",
            "Walk" => "bi-person-walking",
            "Cycle" => "bi-bicycle",
            _ => "bi-flag-fill"
        };

        // Creates a CSS class for the event banner based on the event type.
        // For example, "Run" becomes "banner-run".
        public static string BannerClass(string eventType) => "banner-" + eventType.ToLowerInvariant();

        // Converts a number into its ordinal form.
        // Examples: 1 -> 1st, 2 -> 2nd, 47 -> 47th, 11 -> 11th.
        public static string Ordinal(int n)
        {
            // Gets the last two digits to handle numbers between 11 and 13.
            int lastTwo = n % 100;

            // Numbers ending in 11, 12 or 13 use "th".
            if (lastTwo is >= 11 and <= 13) return n + "th";

            // Determines the correct suffix using the last digit.
            return (n % 10) switch { 1 => n + "st", 2 => n + "nd", 3 => n + "rd", _ => n + "th" };
        }

        // Creates initials from a person's name.
        public static string Initials(string name)
        {
            // Splits the name into separate words and removes empty entries.
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Returns a question mark if no name was provided.
            if (parts.Length == 0) return "?";

            // If there is only one name, returns its first letter.
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();

            // If there are multiple names, combines the first letter of the first
            // and last names, then converts them to uppercase.
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }
}
