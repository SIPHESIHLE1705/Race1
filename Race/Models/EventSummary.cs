namespace Race.Models
{
    // Event plus the number of enrolments (used by the Organiser dashboard)
    public class EventSummary : Event
    {
        public int enrolmentCount { get; set; }
    }
}
