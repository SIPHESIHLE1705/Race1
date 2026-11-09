namespace Race.MVC.Models
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryAge { get; set; }
        public decimal CategoryDistance { get; set; }
        public TimeSpan StartTime { get; set; }
        public int EventID { get; set; }
    }
}
