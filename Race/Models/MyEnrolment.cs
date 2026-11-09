namespace Race.Models
{
    // One row of the Participant "My Enrolments" page
    public class MyEnrolment
    {
        public int enrolmentID { get; set; }
        public DateTime enrolmentDate { get; set; }
        public int eventID { get; set; }
        public string eventName { get; set; } = string.Empty;
        public DateTime eventDate { get; set; }
        public string eventLocation { get; set; } = string.Empty;
        public string eventType { get; set; } = string.Empty;
        public int categoryID { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
