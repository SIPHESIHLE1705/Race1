using Race.MVC.Models;

namespace Race.MVC.ViewModels
{
    public class EventDetailsViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public bool IsLoggedIn { get; set; }
        public string? Role { get; set; }
        public bool AlreadyEnrolled { get; set; }
        public bool IsPast => Event.EventDate.Date < DateTime.Today;
    }
}
