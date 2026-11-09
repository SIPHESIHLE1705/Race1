namespace Race.Models
{
    public class Event
    {
        public int eventID { get; set; }
        public string eventName { get; set; } = string.Empty;
        public string eventDescription { get; set; } = string.Empty;
        public string eventLocation { get; set; } = string.Empty;
        public decimal eventDistance { get; set; }
        public DateTime eventDate { get; set; }
        public string eventType { get; set; } = string.Empty;
        public int organiserID { get; set; }
    }
}