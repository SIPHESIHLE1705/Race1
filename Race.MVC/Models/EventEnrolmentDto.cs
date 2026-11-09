namespace Race.MVC.Models
{
    public class EventEnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public int UserID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ResultID { get; set; }
        public TimeSpan? FinishingTime { get; set; }
        public int? FinishingPosition { get; set; }
    }
}
