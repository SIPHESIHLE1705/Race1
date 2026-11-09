using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;
using Race.MVC.ViewModels;

namespace Race.MVC.Controllers
{
    [SessionAuthorize(Role = Roles.Organiser)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class OrganiserController : BaseController
    {
        private readonly IApiClient _api;

        public OrganiserController(IApiClient api) => _api = api;

        // =====================================================
        // DASHBOARD
        // =====================================================
        public async Task<IActionResult> Dashboard()
        {
            var result = await _api.GetAsync<List<EventSummaryDto>>("api/Events/mine");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(new OrganiserDashboardViewModel
            {
                OrganiserName = HttpContext.Session.GetFullName(),
                Events = result.Data ?? new List<EventSummaryDto>()
            });
        }

        // =====================================================
        // EVENTS (list, create, edit, delete)
        // =====================================================
        public async Task<IActionResult> Events()
        {
            var result = await _api.GetAsync<List<EventSummaryDto>>("api/Events/mine");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(result.Data ?? new List<EventSummaryDto>());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View("EventForm", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View("EventForm", model);

            var result = await _api.PostAsync(EventQuery("api/Events", model));
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            FlashSuccess($"Event \"{model.EventName}\" was created. Now add its categories.");
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var (ev, failure) = await LoadOwnedEventAsync(id);
            if (ev == null) return failure!;

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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventFormViewModel model)
        {
            model.EventID = id;
            if (!ModelState.IsValid) return View("EventForm", model);

            var result = await _api.PutAsync(EventQuery($"api/Events/{id}", model));
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View("EventForm", model);
            }

            FlashSuccess("Event updated successfully.");
            return RedirectToAction(nameof(Events));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _api.DeleteAsync($"api/Events/{id}");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) FlashSuccess("Event deleted.");
            else FlashError(result.Message);   // e.g. "Delete dependent results, enrolments and categories first"

            return RedirectToAction(nameof(Events));
        }

        // =====================================================
        // CATEGORIES (per event)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            return await CategoriesViewAsync(eventId, new CategoryFormViewModel { EventID = eventId });
        }

        // The form fields are named "NewCategory.CategoryName" etc., so bind with that prefix
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(
            [Bind(Prefix = nameof(ManageCategoriesViewModel.NewCategory))] CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return await CategoriesViewAsync(model.EventID, model);

            var result = await _api.PostAsync(CategoryQuery($"api/events/{model.EventID}/categories", model));
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return await CategoriesViewAsync(model.EventID, model);
            }

            FlashSuccess("Category added.");
            return RedirectToAction(nameof(Categories), new { eventId = model.EventID });
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int categoryId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
            var category = categories.Data?.FirstOrDefault(c => c.CategoryID == categoryId);
            if (category == null)
            {
                FlashError("Category not found.");
                return RedirectToAction(nameof(Categories), new { eventId });
            }

            ViewBag.EventName = ev.EventName;
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _api.PutAsync(
                CategoryQuery($"api/events/{model.EventID}/categories/{model.CategoryID}", model));
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            FlashSuccess("Category updated.");
            return RedirectToAction(nameof(Categories), new { eventId = model.EventID });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            var result = await _api.DeleteAsync($"api/events/{eventId}/categories/{categoryId}");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) FlashSuccess("Category deleted.");
            else FlashError(result.Message);

            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // =====================================================
        // ENROLMENTS (who has enrolled for an event)
        // =====================================================
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            var result = await _api.GetAsync<List<EventEnrolmentDto>>($"api/Enrolments/event/{eventId}");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(new EventEnrolmentsViewModel
            {
                Event = ev,
                Enrolments = result.Data ?? new List<EventEnrolmentDto>()
            });
        }

        // =====================================================
        // RESULTS ENTRY (capture + publish finish times and positions)
        // =====================================================
        public async Task<IActionResult> Results(int eventId)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            var result = await _api.GetAsync<List<EventEnrolmentDto>>($"api/Enrolments/event/{eventId}");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(new EventEnrolmentsViewModel
            {
                Event = ev,
                Enrolments = result.Data ?? new List<EventEnrolmentDto>()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(ResultFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                FlashError(FirstModelError());
                return RedirectToAction(nameof(Results), new { eventId = model.EventID });
            }
            if (model.FinishingTime <= TimeSpan.Zero)
            {
                FlashError("Finishing time must be greater than zero.");
                return RedirectToAction(nameof(Results), new { eventId = model.EventID });
            }

            ApiResult result = model.ResultID is > 0
                ? await _api.PutAsync(ApiUrl.Build($"api/Results/{model.ResultID}",
                    ("finishingTime", model.FinishingTime), ("finishingPosition", model.Position)))
                : await _api.PostAsync(ApiUrl.Build("api/Results",
                    ("enrolmentID", model.EnrolmentID),
                    ("finishingTime", model.FinishingTime),
                    ("finishingPosition", model.Position)));

            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) FlashSuccess("Result saved and published.");
            else FlashError(result.Message);

            return RedirectToAction(nameof(Results), new { eventId = model.EventID });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResult(int eventId, int resultId)
        {
            var result = await _api.DeleteAsync($"api/Results/{resultId}");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) FlashSuccess("Result removed.");
            else FlashError(result.Message);

            return RedirectToAction(nameof(Results), new { eventId });
        }

        // =====================================================
        // PRIVATE HELPERS
        // =====================================================

        // Loads an event and confirms it belongs to the logged-in organiser.
        // (The API enforces this too; this just gives a friendly redirect.)
        private async Task<(EventDto? Event, IActionResult? Failure)> LoadOwnedEventAsync(int eventId)
        {
            var result = await _api.GetAsync<EventDto>($"api/Events/{eventId}");

            if (result.IsUnauthorized) return (null, SessionExpired());

            if (!result.Success || result.Data == null || result.Data.OrganiserID != CurrentUserId)
            {
                FlashError(result.Success ? "You can only manage your own events." : result.Message);
                return (null, RedirectToAction(nameof(Events)));
            }

            return (result.Data, null);
        }

        private async Task<IActionResult> CategoriesViewAsync(int eventId, CategoryFormViewModel form)
        {
            var (ev, failure) = await LoadOwnedEventAsync(eventId);
            if (ev == null) return failure!;

            var categories = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");
            if (categories.IsUnauthorized) return SessionExpired();
            if (!categories.Success) ViewBag.ErrorMessage = categories.Message;

            return View("Categories", new ManageCategoriesViewModel
            {
                Event = ev,
                Categories = categories.Data ?? new List<CategoryDto>(),
                NewCategory = form
            });
        }

        private static string EventQuery(string path, EventFormViewModel m) => ApiUrl.Build(path,
            ("eventName", m.EventName.Trim()),
            ("eventDescription", m.EventDescription.Trim()),
            ("eventLocation", m.EventLocation.Trim()),
            ("eventDistance", m.EventDistance),
            ("eventDate", m.EventDate),
            ("eventType", m.EventType));

        private static string CategoryQuery(string path, CategoryFormViewModel m) => ApiUrl.Build(path,
            ("categoryName", m.CategoryName.Trim()),
            ("categoryAge", m.CategoryAge),
            ("categoryDistance", m.CategoryDistance),
            ("startTime", m.StartTime));
    }
}
