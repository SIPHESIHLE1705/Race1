
using Microsoft.AspNetCore.Mvc;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;
using System.Diagnostics;

namespace Race.MVC.Controllers
{
    // Handles the public home page and application error pages.
    public class HomeController : BaseController
    {
        // API client used to retrieve event information from the API.
        private readonly IApiClient _api;

        // Constructor receives the API client through dependency injection.
        public HomeController(IApiClient api) => _api = api;

        // ---------- HOME PAGE ----------

        // Displays upcoming events and allows users to filter the events.
        // This page is accessible to everyone.
        public async Task<IActionResult> Index([FromQuery] EventFilterViewModel filter)
        {
            // Clears validation errors caused by incorrectly typed filter values.
            ModelState.Clear();

            // Creates the home page view model and stores the selected filters.
            var vm = new HomeViewModel { Filter = filter };

            // Retrieves all events from the API.
            var result = await _api.GetAsync<List<EventDto>>("api/Events");

            // Displays an error message if the events cannot be retrieved.
            if (!result.Success)
            {
                vm.ErrorMessage = result.Message;
                return View(vm);
            }

            // Starts with events scheduled for today or a future date.
            IEnumerable<EventDto> query = (result.Data ?? new List<EventDto>())
                .Where(e => e.EventDate.Date >= DateTime.Today);

            // Filters events by name, location or description when a search is provided.
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string s = filter.Search.Trim();
                query = query.Where(e =>
                    e.EventName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.EventLocation.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.EventDescription.Contains(s, StringComparison.OrdinalIgnoreCase));
            }

            // Filters events according to the selected event type.
            if (!string.IsNullOrWhiteSpace(filter.EventType))
                query = query.Where(e => e.EventType == filter.EventType);

            // Displays events occurring on or after the selected start date.
            if (filter.FromDate.HasValue)
                query = query.Where(e => e.EventDate.Date >= filter.FromDate.Value.Date);

            // Filters out events whose distance exceeds the selected maximum distance.
            if (filter.MaxDistance.HasValue)
                query = query.Where(e => e.EventDistance <= filter.MaxDistance.Value);

            // Sorts the filtered events by date, starting with the earliest event.
            vm.Events = query.OrderBy(e => e.EventDate).ToList();

            // For participants, identifies events they have already enrolled in.
            if (CurrentRole == Roles.Participant)
            {
                // Retrieves the logged-in participant's enrolments.
                var mine = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");

                // Stores the IDs of enrolled events if the API request succeeds.
                if (mine.Success && mine.Data != null)
                    vm.EnrolledEventIds = mine.Data.Select(m => m.EventID).ToHashSet();
            }

            // Passes the home page information and filtered events to the view.
            return View(vm);
        }

        // ---------- ERROR PAGE ----------

        // Disables browser caching for the error page.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Creates an error view model containing the current request ID.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ---------- HTTP ERROR ----------

        // Handles HTTP errors such as 404 when configured in Program.cs.
        public IActionResult HttpError(int code)
        {
            // Displays a specific message when the requested page is not found.
            var vm = code == 404
                ? new ErrorViewModel { Title = "Page not found", Message = "We couldn't find the page you were looking for." }
                : new ErrorViewModel();

            // Returns the Error view with the appropriate error information.
            return View("Error", vm);
        }
    }
}

