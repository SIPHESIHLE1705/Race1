using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;

namespace Race.MVC.Controllers
{
    // Public event details + participant enrolment
    public class EventsController : BaseController
    {
        private readonly IApiClient _api;

        public EventsController(IApiClient api) => _api = api;

        // ---------- EVENT DETAILS (open to everyone) ----------
        public async Task<IActionResult> Details(int id)
        {
            var eventResult = await _api.GetAsync<EventDto>($"api/Events/{id}");
            if (!eventResult.Success || eventResult.Data == null)
            {
                FlashError(eventResult.Message);
                return RedirectToAction("Index", "Home");
            }

            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");

            var vm = new EventDetailsViewModel
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>(),
                IsLoggedIn = HttpContext.Session.IsLoggedIn(),
                Role = CurrentRole
            };

            if (CurrentRole == Roles.Participant)
            {
                var mine = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");
                vm.AlreadyEnrolled = mine.Data?.Any(m => m.EventID == id) ?? false;
            }

            return View(vm);
        }

        // ---------- ENROL (participants only) ----------
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Role = Roles.Participant)]
        public async Task<IActionResult> Enrol(int eventID, int categoryID)
        {
            if (categoryID <= 0)
            {
                FlashError("Please select a category before enrolling.");
                return RedirectToAction(nameof(Details), new { id = eventID });
            }

            var result = await _api.PostAsync(ApiUrl.Build("api/Enrolments",
                ("eventID", eventID), ("categoryID", categoryID)));

            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                FlashError(result.Message);
                return RedirectToAction(nameof(Details), new { id = eventID });
            }

            FlashSuccess("You're in! Your enrolment has been recorded.");
            return RedirectToAction("MyEnrolments", "Participant");
        }
    }
}
