using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;

namespace Race.MVC.Controllers
{
    // Restricts access to users who are logged in as Participants.
    // Prevents the browser from storing cached versions of pages.
    [SessionAuthorize(Role = Roles.Participant)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ParticipantController : BaseController
    {
        // API client used to communicate with the RaceDay API.
        private readonly IApiClient _api;

        // Receives the API client through dependency injection.
        public ParticipantController(IApiClient api) => _api = api;

        // ---------- MY ENROLMENTS ----------

        // Displays all enrolments belonging to the logged-in participant.
        public async Task<IActionResult> MyEnrolments()
        {
            // Requests the participant's enrolment details from the API.
            var result = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");

            // Redirects the participant if their session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error message if the API request was unsuccessful.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Sends the enrolment list to the MyEnrolments view.
            // Returns an empty list if the API does not provide any data.
            return View(result.Data ?? new List<MyEnrolmentDto>());
        }

        // Cancels an enrolment selected by the participant.
        // POST is used because this action changes data.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            // Sends a DELETE request to the API for the selected enrolment.
            var result = await _api.DeleteAsync($"api/Enrolments/{id}");

            // Redirects the participant if their session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays a success or error message depending on the API response.
            if (result.Success) FlashSuccess("Your enrolment has been cancelled.");
            else FlashError(result.Message);

            // Returns the participant to their enrolment list.
            return RedirectToAction(nameof(MyEnrolments));
        }

        // ---------- MY RESULTS ----------

        // Displays the results for the logged-in participant.
        public async Task<IActionResult> MyResults()
        {
            // Requests the participant's results from the API.
            var result = await _api.GetAsync<List<MyResultDto>>("api/Results/my");

            // Redirects the participant if their session has expired.
            if (result.IsUnauthorized) return SessionExpired();

            // Displays an error message if the API request was unsuccessful.
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            // Sends the participant's results to the MyResults view.
            // Returns an empty list if no results are available.
            return View(result.Data ?? new List<MyResultDto>());
        }
    }
}
