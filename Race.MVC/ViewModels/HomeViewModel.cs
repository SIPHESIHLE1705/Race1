using Race.MVC.Models;

namespace Race.MVC.ViewModels
{
    public class HomeViewModel
    {
        public EventFilterViewModel Filter { get; set; } = new();
        public List<EventDto> Events { get; set; } = new();
        public HashSet<int> EnrolledEventIds { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
