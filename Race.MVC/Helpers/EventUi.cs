namespace Race.MVC.Helpers
{
    // Small display helpers shared by the views
    public static class EventUi
    {
        public static string Icon(string eventType) => eventType switch
        {
            "Run" => "bi-lightning-charge-fill",
            "Walk" => "bi-person-walking",
            "Cycle" => "bi-bicycle",
            _ => "bi-flag-fill"
        };

        public static string BannerClass(string eventType) => "banner-" + eventType.ToLowerInvariant();

        // 1 -> 1st, 2 -> 2nd, 47 -> 47th, 11 -> 11th
        public static string Ordinal(int n)
        {
            int lastTwo = n % 100;
            if (lastTwo is >= 11 and <= 13) return n + "th";
            return (n % 10) switch { 1 => n + "st", 2 => n + "nd", 3 => n + "rd", _ => n + "th" };
        }

        public static string Initials(string name)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0][..1].ToUpperInvariant();
            return (parts[0][..1] + parts[^1][..1]).ToUpperInvariant();
        }
    }
}
