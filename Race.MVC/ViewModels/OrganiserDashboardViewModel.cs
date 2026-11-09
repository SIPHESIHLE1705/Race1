using Race.MVC.Models;

namespace Race.MVC.ViewModels
{
    public class OrganiserDashboardViewModel
    {
        public string OrganiserName { get; set; } = string.Empty;
        public List<EventSummaryDto> Events { get; set; } = new();

        public int TotalEvents => Events.Count;
        public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);

        public List<EventSummaryDto> UpcomingEvents => Events
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .OrderBy(e => e.EventDate)
            .ToList();

        public int MaxEnrolments => Events.Count == 0 ? 1 : Math.Max(1, Events.Max(e => e.EnrolmentCount));
    }
}
