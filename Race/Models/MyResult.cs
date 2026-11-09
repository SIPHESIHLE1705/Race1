namespace Race.Models
{
    // Represents one row displayed on the participant's My Results page.
    public class MyResult
    {
        // Unique ID of the result.
        public int resultID { get; set; }

        // ID of the event the participant took part in.
        public int eventID { get; set; }

        // Name of the event.
        public string eventName { get; set; } = string.Empty;

        // Date when the event took place.
        public DateTime eventDate { get; set; }

        // Name of the category the participant entered.
        public string categoryName { get; set; } = string.Empty;

        // Time taken by the participant to finish the event.
        public TimeSpan finishingTime { get; set; }

        // Participant's finishing position in the event.
        public int finishingPosition { get; set; }

        // Total number of participants who finished the event.
        public int totalFinishers { get; set; }
    }
}
