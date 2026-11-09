using Race.MVC.Models;

namespace Race.MVC.ViewModels
{
    // Used by the Enrolments page and the Results entry page
    public class EventEnrolmentsViewModel
    {
        public EventDto Event { get; set; } = new();
        public List<EventEnrolmentDto> Enrolments { get; set; } = new();
    }
}
