
using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;

namespace Race.MVC.Controllers
{
    // Handles event details and participant enrolment.
    public class EventsController : BaseController
    {
        // API client used to communicate with the API.
        private readonly IApiClient _api;

        // Constructor receives the API client through dependency injection.
        public EventsController(IApiClient api) => _api = api;

        // ---------- EVENT DETAILS (open to everyone) ----------

        // Retrieves and displays the details of a selected event.
        public async Task<IActionResult> Details(int id)
        {
            // Requests the selected event's details from the API.
            var eventResult = await _api.GetAsync<EventDto>($"api/Events/{id}");

            // Redirects to the home page if the event cannot be found or retrieved.
            if (!eventResult.Success || eventResult.Data == null)
            {
                FlashError(eventResult.Message);
                return RedirectToAction("Index", "Home");
            }

            // Retrieves the categories belonging to the selected event.
            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");

            // Creates a view model containing event details and its categories.
            var vm = new EventDetailsViewModel
            {
                Event = eventResult.Data,
                Categories = categories.Data ?? new List<CategoryDto>(),

                // Checks whether the current user is logged in.
                IsLoggedIn = HttpContext.Session.IsLoggedIn(),

                // Gets the current user's role from the session.
                Role = CurrentRole
            };

            // Checks whether the logged-in user is a participant.
            if (CurrentRole == Roles.Participant)
            {
                // Retrieves the participant's existing enrolments.
                var mine = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");

                // Checks whether the participant has already enrolled in this event.
                vm.AlreadyEnrolled = mine.Data?.Any(m => m.EventID == id) ?? false;
            }

            // Passes the event details and categories to the view.
            return View(vm);
        }

        // ---------- ENROL (participants only) ----------

        // Processes a participant's request to enrol in an event category.
        [HttpPost, ValidateAntiForgeryToken, SessionAuthorize(Role = Roles.Participant)]
        public async Task<IActionResult> Enrol(int eventID, int categoryID)
        {
            // Checks whether a valid category has been selected.
            if (categoryID <= 0)
            {
                FlashError("Please select a category before enrolling.");

                // Returns the participant to the event details page.
                return RedirectToAction(nameof(Details), new { id = eventID });
            }

            // Sends the event and category IDs to the API to create the enrolment.
            var result = await _api.PostAsync(ApiUrl.Build("api/Enrolments",
                ("eventID", eventID), ("categoryID", categoryID)));

            // Redirects the participant if their API session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the enrolment was unsuccessful.
            if (!result.Success)
            {
                FlashError(result.Message);
                return RedirectToAction(nameof(Details), new { id = eventID });
            }

            // Displays a success message after the enrolment is recorded.
            FlashSuccess("You're in! Your enrolment has been recorded.");

            // Redirects the participant to their enrolments page.
            return RedirectToAction("MyEnrolments", "Participant");
        }
    }
}

