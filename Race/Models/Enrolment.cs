namespace Race.Models
{
    public class Enrolment
    {
        public int enrolmentID { get; set; }
        public DateTime enrolmentDate { get; set; }
        public int userID { get; set; }
        public int eventID { get; set; }
        public int categoryID { get; set; }
    }
}
