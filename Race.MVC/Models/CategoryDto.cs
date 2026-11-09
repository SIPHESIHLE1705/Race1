namespace Race.MVC.Models
{
    // Represents a category belonging to a particular event.
    public class CategoryDto
    {
        // Stores the unique ID of the category.
        public int CategoryID { get; set; }

        // Stores the name of the category.
        public string CategoryName { get; set; } = string.Empty;

        // Stores the minimum or required age for the category.
        public int CategoryAge { get; set; }

        // Stores the distance of the category.
        public decimal CategoryDistance { get; set; }

        // Stores the scheduled starting time of the category.
        public TimeSpan StartTime { get; set; }

        // Stores the ID of the event to which the category belongs.
        public int EventID { get; set; }
    }
}
