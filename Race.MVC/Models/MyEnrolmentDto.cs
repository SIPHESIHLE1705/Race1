namespace Race.MVC.Models
{
    public class MyEnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
