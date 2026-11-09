using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly store_info db = new store_info();

        [HttpPost]
        public IActionResult CreateEnrolment(
            [FromQuery] int eventID,
            [FromQuery] int categoryID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (role != "Participant")
                return Forbid();

            if (eventID <= 0 || categoryID <= 0)
                return BadRequest("Valid event and category IDs are required.");

            string result = db.createEnrolment(
                userID, eventID, categoryID);

            if (result == "CATEGORY_NOT_FOUND")
                return BadRequest("Category does not belong to this event.");

            if (result == "ALREADY_ENROLLED")
                return Conflict("You are already enrolled in this event.");

            if (result == "SUCCESS")
                return Ok("Enrolment successful.");

            return StatusCode(500, "Enrolment failed.");
        }

        [HttpGet("my")]
        public IActionResult GetMyEnrolments()
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;

            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (HttpContext.Session.GetString("userRole") != "Participant")
                return Forbid();

            return Ok(db.getEnrolments(userID));
        }

        // PART 3: participant's enrolments with event/category names and status
        [HttpGet("my/details")]
        public IActionResult GetMyEnrolmentDetails()
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            if (userID == 0)
                return Unauthorized("Please log in first.");
            if (HttpContext.Session.GetString("userRole") != "Participant")
                return StatusCode(StatusCodes.Status403Forbidden, "Only participants can do that.");

            return Ok(db.getMyEnrolmentDetails(userID));
        }

        // PART 3: organiser views everyone enrolled in one of their events
        [HttpGet("event/{eventID:int}")]
        public IActionResult GetEventEnrolments([FromRoute] int eventID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            if (userID == 0)
                return Unauthorized("Please log in first.");
            if (HttpContext.Session.GetString("userRole") != "Organiser")
                return StatusCode(StatusCodes.Status403Forbidden, "Only organisers can do that.");
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            return Ok(db.getEventEnrolments(eventID));
        }

        [HttpDelete("{enrolmentID:int}")]
        public IActionResult DeleteEnrolment([FromRoute] int enrolmentID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Participant") return Forbid();
            if (enrolmentID <= 0) return BadRequest("Enter a valid enrolment ID.");

            string result = db.deleteEnrolment(enrolmentID, userID);
            if (result == "SUCCESS") return Ok("Enrolment deleted successfully.");
            if (result == "NOT_FOUND_OR_HAS_RESULT")
                return Conflict("Enrolment was not found, does not belong to you, or already has a result.");
            return StatusCode(500, "Could not delete enrolment.");
        }

    }
}