using Microsoft.AspNetCore.Mvc;
using Race.MVC.Filters;
using Race.MVC.Helpers;
using Race.MVC.Models;
using Race.MVC.Services;

namespace Race.MVC.Controllers
{
    [SessionAuthorize(Role = Roles.Participant)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ParticipantController : BaseController
    {
        private readonly IApiClient _api;

        public ParticipantController(IApiClient api) => _api = api;

        // ---------- MY ENROLMENTS ----------
        public async Task<IActionResult> MyEnrolments()
        {
            var result = await _api.GetAsync<List<MyEnrolmentDto>>("api/Enrolments/my/details");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(result.Data ?? new List<MyEnrolmentDto>());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var result = await _api.DeleteAsync($"api/Enrolments/{id}");
            if (result.IsUnauthorized) return SessionExpired();

            if (result.Success) FlashSuccess("Your enrolment has been cancelled.");
            else FlashError(result.Message);

            return RedirectToAction(nameof(MyEnrolments));
        }

        // ---------- MY RESULTS ----------
        public async Task<IActionResult> MyResults()
        {
            var result = await _api.GetAsync<List<MyResultDto>>("api/Results/my");
            if (result.IsUnauthorized) return SessionExpired();
            if (!result.Success) ViewBag.ErrorMessage = result.Message;

            return View(result.Data ?? new List<MyResultDto>());
        }
    }
}
