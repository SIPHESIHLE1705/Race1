namespace Race.Models
{
    // One row of the Participant "My Results" page
    public class MyResult
    {
        public int resultID { get; set; }
        public int eventID { get; set; }
        public string eventName { get; set; } = string.Empty;
        public DateTime eventDate { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public TimeSpan finishingTime { get; set; }
        public int finishingPosition { get; set; }
        public int totalFinishers { get; set; }
    }
}
