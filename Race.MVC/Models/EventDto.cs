namespace Race.MVC.Models
{
    public class EventDto
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public decimal EventDistance { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int OrganiserID { get; set; }
    }
}
