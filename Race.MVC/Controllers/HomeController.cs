using Microsoft.AspNetCore.Mvc;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;
using System.Diagnostics;

namespace Race.MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IApiClient _api;

        public HomeController(IApiClient api) => _api = api;

        // Home page: upcoming events + filters (open to everyone)
        public async Task<IActionResult> Index([FromQuery] EventFilterViewModel filter)
        {
            ModelState.Clear();   // ignore badly typed filter values instead of failing

            var vm = new HomeViewModel { Filter = filter };

            var result = await _api.GetAsync<List<EventDto>>("api/Events");
            if (!result.Success)
            {
                vm.ErrorMessage = result.Message;
                return View(vm);
            }

            IEnumerable<EventDto> query = (result.Data ?? new List<EventDto>())
                .Where(e => e.EventDate.Date >= DateTime.Today);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string s = filter.Search.Trim();
                query = query.Where(e =>
                    e.EventName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.EventLocation.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                    e.EventDescription.Contains(s, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(filter.EventType))
                query = query.Where(e => e.EventType == filter.EventType);
            if (filter.FromDate.HasValue)
                query = query.Where(e => e.EventDate.Date >= filter.FromDate.Value.Date);
            if (filter.MaxDistance.HasValue)
                query = query.Where(e => e.EventDistance <= filter.MaxDistance.Value);

            vm.Events = query.OrderBy(e => e.EventDate).ToList();

            // For participants, mark the events they are already enrolled in
            if (CurrentRole == Roles.Participant)
            {
                var mine = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");
                if (mine.Success && mine.Data != null)
                    vm.EnrolledEventIds = mine.Data.Select(m => m.EventID).ToHashSet();
            }

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Handles 404 etc. (wired up with UseStatusCodePagesWithReExecute in Program.cs)
        public IActionResult HttpError(int code)
        {
            var vm = code == 404
                ? new ErrorViewModel { Title = "Page not found", Message = "We couldn't find the page you were looking for." }
                : new ErrorViewModel();
            return View("Error", vm);
        }
    }
}
