namespace Race.Models
{
    public class CreateCategoryRequest
    {
        public string categoryName { get; set; } = string.Empty;
        public int categoryAge { get; set; }
        public decimal categoryDistance { get; set; }
        public TimeSpan startTime { get; set; }
    }
}