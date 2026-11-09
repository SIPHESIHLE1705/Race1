namespace Race.Models
{
    // One row of the Organiser "Enrolments" and "Results entry" pages
    public class EventEnrolment
    {
        public int enrolmentID { get; set; }
        public DateTime enrolmentDate { get; set; }
        public int userID { get; set; }
        public string participantName { get; set; } = string.Empty;
        public string participantEmail { get; set; } = string.Empty;
        public int categoryID { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public int? resultID { get; set; }
        public TimeSpan? finishingTime { get; set; }
        public int? finishingPosition { get; set; }
    }
}
