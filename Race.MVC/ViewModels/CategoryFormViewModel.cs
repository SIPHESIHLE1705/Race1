using System.ComponentModel.DataAnnotations;

namespace Race.MVC.ViewModels
{
    // Used by Add Category and Edit Category
    public class CategoryFormViewModel
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(50, ErrorMessage = "Category name can be at most 50 characters.")]
        [Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0, 120, ErrorMessage = "Minimum age must be between 0 and 120.")]
        [Display(Name = "Minimum age")]
        public int CategoryAge { get; set; }

        [Range(0.01, 1000, ErrorMessage = "Distance must be between 0.01 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal CategoryDistance { get; set; }

        [Required(ErrorMessage = "Please enter the start time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Start time")]
        public TimeSpan? StartTime { get; set; }
    }
}
