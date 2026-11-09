namespace Race.Models
{
    public class Category
    {
        public int categoryID { get; set; }
        public string categoryName { get; set; } = string.Empty;
        public int categoryAge { get; set; }
        public decimal categoryDistance { get; set; }
        public TimeSpan startTime { get; set; }
        public int eventID { get; set; }
    }
}
