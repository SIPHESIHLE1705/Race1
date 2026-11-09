namespace Race.Models
{
    public class Result
    {
        public int resultID { get; set; }
        public TimeSpan finishingTime { get; set; }
        public int finishingPosition { get; set; }
        public int enrolmentID { get; set; }
    }
}