
using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;

namespace Race.MVC.Controllers
{
    // Restricts all actions in this controller to logged-in organisers.
    // Disables caching to prevent sensitive pages from being stored by the browser.
    [SessionAuthorize(Role = Roles.Organiser)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class OrganiserController : BaseController
    {
        // API client used to communicate with the API.
        private readonly IApiClient _api;

        // Constructor receives the API client through dependency injection.
        public OrganiserController(IApiClient api) => _api = api;

        // =====================================================
        // DASHBOARD
        // =====================================================

        // Retrieves and displays the organiser's dashboard information.
        public async Task<IActionResult> Dashboard()
        {
            // Retrieves events belonging to the logged-in organiser.
            var result = await _api.GetAsync<List<EventSummaryDto>>("api/Events/mine");

            // Redirects the organiser if their API session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Stores an error message if the events could not be retrieved.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Creates the dashboard view model with the organiser's name and events.
            return View(new OrganiserDashboardViewModel
            {
                OrganiserName = HttpContext.Session.GetFullName(),
                Events = result.Data ?? new List<EventSummaryDto>()
            });
        }

        // =====================================================
        // EVENTS (list, create, edit, delete)
        // =====================================================

        // Retrieves and displays events belonging to the logged-in organiser.
        public async Task<IActionResult> Events()
        {
            // Requests the organiser's events from the API.
            var result = await _api.GetAsync<List<EventSummaryDto>>("api/Events/mine");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the events cannot be loaded.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Passes the retrieved events to the view.
            return View(result.Data ?? new List<EventSummaryDto>());
        }

        // Displays an empty form for creating a new event.
        [HttpGet]
        public IActionResult CreateEvent()
        {
            // Sets the default event date to 30 days from today.
            return View("EventForm", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30) });
        }

        // Processes the form and creates a new event.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            // Checks whether the submitted event details are valid.
            if (!ModelState.IsValid) return View("EventForm", model);

            // Sends the new event details to the API.
            var result = await _api.PostAsync(EventQuery("api/Events", model));

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays validation errors if the event could not be created.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            // Confirms event creation and redirects to the event list.
            FlashSuccess($"Event \"{model.EventName}\" was created. Now add its categories.");
            return RedirectToAction(nameof(Events));
        }

        // Retrieves an existing event and displays its details in the edit form.
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            // Loads the event and checks that it belongs to the current organiser.
            var (ev, failure) = await LoadOwnedEventAsync(id);
            if (ev == null) return failure!;

            // Copies the existing event details into the form model.
            return View("EventForm", new EventFormViewModel
            {
                EventID = ev.EventID,
                EventName = ev.EventName,
                EventDescription = ev.EventDescription,
                EventLocation = ev.EventLocation,
                EventDistance = ev.EventDistance,
                EventDate = ev.EventDate,
                EventType = ev.EventType
            });
        }

        // Processes changes submitted through the event edit form.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormViewModel model)
        {
            // Assigns the event ID received from the route to the model.
            model.EventID = id;

            // Checks whether the updated event details are valid.
            if (!ModelState.IsValid) return View("EventForm", model);

            // Sends the updated event details to the API.
            var result = await _api.PutAsync(EventQuery($"api/Events/{id}", model));

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the event could not be updated.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            // Confirms the update and returns to the event list.
            FlashSuccess("Event updated successfully.");
            return RedirectToAction(nameof(Events));
        }

        // Deletes an event belonging to the logged-in organiser.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            // Sends a delete request to the API for the selected event.
            var result = await _api.DeleteAsync($"api/Events/{id}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays a success or error message depending on the API response.
            if (result.Success) FlashSuccess("Event deleted.");
            else FlashError(result.Message);   // e.g. "Delete dependent results, enrolments and categories first"

            // Returns to the organiser's event list.
            return RedirectToAction(nameof(Events));
        }

        // =====================================================
        // CATEGORIES (per event)
        // =====================================================

        // Displays the categories belonging to a selected event.
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            // Loads the categories page with an empty category form.
            return await CategoriesViewAsync(eventId, new CategoryFormViewModel { EventID = eventId });
        }

        // Processes the form for adding a new category.
        // The form uses NewCategory as the prefix for its submitted fields.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(
            [Bind(Prefix = nameof(ManageCategoriesViewModel.NewCategory))] CategoryFormViewModel model)
        {
            // Checks whether the new category details are valid.
            if (!ModelState.IsValid) return await CategoriesViewAsync(model.EventID, model);

            // Sends the new category details to the API for the selected event.
            var result = await _api.PostAsync(CategoryQuery($"api/events/{model.EventID}/categories", model));

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the category could not be added.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return await CategoriesViewAsync(model.EventID, model);
            }

            // Confirms the category was added and reloads the categories page.
            FlashSuccess("Category added.");
            return RedirectToAction(nameof(Categories), new { eventId = model.EventID });
        }

        // Retrieves an existing category and displays its details for editing.
        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int categoryId)
        {
            // Checks that the selected event belongs to the current organiser.
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            // Retrieves the categories belonging to the selected event.
            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");

            // Finds the category selected for editing.
            var category = categories.Data?.FirstOrDefault(c => c.CategoryID == categoryId);

            // Redirects back if the selected category cannot be found.
            if (category == null)
            {
                FlashError("Category not found.");
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            // Passes the event name to the view.
            ViewBag.EventName = ev.EventName;

            // Copies the existing category details into the edit form.
            return View(new CategoryFormViewModel
            {
                CategoryID = category.CategoryID,
                EventID = eventId,
                CategoryName = category.CategoryName,
                CategoryAge = category.CategoryAge,
                CategoryDistance = category.CategoryDistance,
                StartTime = category.StartTime
            });
        }

        // Processes changes submitted through the category edit form.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
        {
            // Checks whether the updated category details are valid.
            if (!ModelState.IsValid) return View(model);

            // Sends the updated category information to the API.
            var result = await _api.PutAsync(
                CategoryQuery($"api/events/{model.EventID}/categories/{model.CategoryID}", model));

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the category could not be updated.
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            // Confirms the update and returns to the event's categories.
            FlashSuccess("Category updated.");
            return RedirectToAction(nameof(Categories), new { eventId = model.EventID });
        }

        // Deletes a category belonging to a selected event.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            // Sends a delete request for the selected category to the API.
            var result = await _api.DeleteAsync($"api/events/{eventId}/categories/{categoryId}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays a success or error message based on the API response.
            if (result.Success) FlashSuccess("Category deleted.");
            else FlashError(result.Message);

            // Returns to the categories page for the selected event.
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // =====================================================
        // ENROLMENTS (who has enrolled for an event)
        // =====================================================

        // Retrieves and displays participants enrolled in a selected event.
        public async Task<IActionResult> Enrolments(int eventId)
        {
            // Loads the event and checks that it belongs to the organiser.
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            // Retrieves the enrolments recorded for the selected event.
            var result = await _api.GetAsync<List<EventEnrolmentDto>>($"api/Enrolments/event/{eventId}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the enrolments cannot be retrieved.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Passes the event and its enrolments to the view.
            return View(new EventEnrolmentsViewModel
            {
                Event = ev,
                Enrolments = result.Data ?? new List<EventEnrolmentDto>()
            });
        }

        // =====================================================
        // RESULTS ENTRY (capture + publish finish times and positions)
        // =====================================================

        // Displays the participants enrolled in an event so the organiser
        // can record or manage their race results.
        public async Task<IActionResult> Results(int eventId)
        {
            // Loads the event and checks that it belongs to the organiser.
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            // Retrieves the participants enrolled in the selected event.
            var result = await _api.GetAsync<List<EventEnrolmentDto>>($"api/Enrolments/event/{eventId}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error if the enrolments cannot be retrieved.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Passes the event and its enrolments to the results view.
            return View(new EventEnrolmentsViewModel
            {
                Event = ev,
                Enrolments = result.Data ?? new List<EventEnrolmentDto>()
            });
        }

        // Saves a participant's finishing time and position.
        // Creates a new result or updates an existing result.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(ResultFormViewModel model)
        {
            // Checks whether the submitted result details are valid.
            if (!ModelState.IsValid)
            {
                FlashError(FirstModelError());
                return RedirectToAction(nameof(Results), new { eventId = model.EventID });
            }

            // Ensures that the finishing time is greater than zero.
            if (model.FinishingTime <= TimeSpan.Zero)
            {
                FlashError("Finishing time must be greater than zero.");
                return RedirectToAction(nameof(Results), new { eventId = model.EventID });
            }

            // Updates an existing result if a ResultID is provided.
            // Otherwise, creates a new result for the participant's enrolment.
            ApiResult result = model.ResultID is > 0
                ? await _api.PutAsync(ApiUrl.Build($"api/Results/{model.ResultID}",
                    ("finishingTime", model.FinishingTime), ("finishingPosition", model.Position)))
                : await _api.PostAsync(ApiUrl.Build("api/Results",
                    ("enrolmentID", model.EnrolmentID),
                    ("finishingTime", model.FinishingTime),
                    ("finishingPosition", model.Position)));

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays a success or error message depending on the API response.
            if (result.Success) FlashSuccess("Result saved and published.");
            else FlashError(result.Message);

            // Returns to the results page for the selected event.
            return RedirectToAction(nameof(Results), new { eventId = model.EventID });
        }

        // Deletes a recorded result.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResult(int eventId, int resultId)
        {
            // Sends a delete request for the selected result to the API.
            var result = await _api.DeleteAsync($"api/Results/{resultId}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays a success or error message depending on the API response.
            if (result.Success) FlashSuccess("Result removed.");
            else FlashError(result.Message);

            // Returns to the results page for the selected event.
            return RedirectToAction(nameof(Results), new { eventId });
        }

        // =====================================================
        // PRIVATE HELPERS
        // =====================================================

        // Loads an event and confirms that it belongs to the logged-in organiser.
        // The API also enforces ownership; this method provides a friendly redirect.
        private async Task<(EventDto? Event, IActionResult? Failure)> LoadOwnedEventAsync(int eventId)
        {
            // Retrieves the selected event from the API.
            var result = await _api.GetAsync<EventDto>($"api/Events/{eventId}");

            // Handles an expired API session.
            if (result.IsUnauthorized) return (null, SessionExpired());

            // Checks whether the event exists and belongs to the current organiser.
            if (!result.Success || result.Data == null || result.Data.OrganiserID != CurrentUserId)
            {
                FlashError(result.Success ? "You can only manage your own events." : result.Message);
                return (null, RedirectToAction(nameof(Events)));
            }

            // Returns the event when it belongs to the current organiser.
            return (result.Data, null);
        }

        // Retrieves categories for an event and prepares the categories view.
        private async Task<IActionResult> CategoriesViewAsync(int eventId, CategoryFormViewModel form)
        {
            // Loads the event and checks organiser ownership.
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            // Retrieves the categories belonging to the event.
            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");

            // Handles an expired API session.
            if (categories.IsUnauthorized) return SessionExpired();

            // Displays an error if the categories could not be retrieved.
            if (!categories.Success) ViewBag.ErrorMessage = categories.Message;

            // Creates the view model containing the event, categories and category form.
            return View("Categories", new ManageCategoriesViewModel
            {
                Event = ev,
                Categories = categories.Data ?? new List<CategoryDto>(),
                NewCategory = form
            });
        }

        // Builds the API query string containing the event form details.
        // Trims text fields before sending them to the API.
        private static string EventQuery(string path, EventFormViewModel m) => ApiUrl.Build(path,
            ("eventName", m.EventName.Trim()),
            ("eventDescription", m.EventDescription.Trim()),
            ("eventLocation", m.EventLocation.Trim()),
            ("eventDistance", m.EventDistance),
            ("eventDate", m.EventDate),
            ("eventType", m.EventType));

        // Builds the API query string containing the category form details.
        // Trims the category name before sending it to the API.
        private static string CategoryQuery(string path, CategoryFormViewModel m) => ApiUrl.Build(path,
            ("categoryName", m.CategoryName.Trim()),
            ("categoryAge", m.CategoryAge),
            ("categoryDistance", m.CategoryDistance),
            ("startTime", m.StartTime));
    }
}

