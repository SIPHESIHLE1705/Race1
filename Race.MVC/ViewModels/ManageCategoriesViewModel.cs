using Race.MVC.Models;

namespace Race.MVC.ViewModels
{
    public class ManageCategoriesViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public CategoryFormViewModel NewCategory { get; set; } = new();
    }
}
