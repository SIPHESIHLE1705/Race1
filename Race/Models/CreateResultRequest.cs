namespace Race.Models
{
    public class CreateResultRequest
    {
        public int enrolmentID { get; set; }
        public TimeSpan finishingTime { get; set; }
        public int finishingPosition { get; set; }
    }
}