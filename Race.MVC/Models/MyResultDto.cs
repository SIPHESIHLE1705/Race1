namespace Race.MVC.Models
{
    public class MyResultDto
    {
        public int ResultID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishingTime { get; set; }
        public int FinishingPosition { get; set; }
        public int TotalFinishers { get; set; }
    }
}
