using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly store_info db = new store_info();

        [HttpGet]
        public IActionResult GetResults()
        {
            return Ok(db.getResults());
        }

        // PART 3: participant's personal race history with finisher totals
        [HttpGet("my")]
        public IActionResult GetMyResults()
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            if (userID == 0)
                return Unauthorized("Please log in first.");
            if (HttpContext.Session.GetString("userRole") != "Participant")
                return StatusCode(StatusCodes.Status403Forbidden, "Only participants can do that.");

            return Ok(db.getMyResults(userID));
        }

        [HttpPost]
        public IActionResult CreateResult(
            [FromQuery] int enrolmentID,
            [FromQuery] TimeSpan finishingTime,
            [FromQuery] int finishingPosition)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (role != "Organiser")
                return Forbid();

            if (enrolmentID <= 0 || finishingPosition <= 0)
                return BadRequest("Enter valid enrolment and finishing position.");

            if (finishingTime < TimeSpan.Zero ||
                finishingTime >= TimeSpan.FromDays(1))
            {
                return BadRequest("Enter a valid finishing time.");
            }

            int? eventID = db.getEventIDForEnrolment(enrolmentID);

            if (eventID == null)
                return NotFound("Enrolment not found.");

            if (!db.organiserOwnsEvent(eventID.Value, userID))
                return Forbid();

            CreateResultRequest request = new CreateResultRequest
            {
                enrolmentID = enrolmentID,
                finishingTime = finishingTime,
                finishingPosition = finishingPosition
            };

            string result = db.createResult(request);

            if (result == "RESULT_EXISTS")
                return Conflict("A result already exists for this enrolment.");

            if (result == "SUCCESS")
                return Ok("Result created successfully.");

            return StatusCode(500, "Could not create result.");
        }

        [HttpPut("{resultID:int}")]
        public IActionResult UpdateResult(
            [FromRoute] int resultID,
            [FromQuery] TimeSpan finishingTime,
            [FromQuery] int finishingPosition)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (resultID <= 0 || finishingPosition <= 0)
                return BadRequest("Enter a valid result ID and finishing position.");
            if (finishingTime < TimeSpan.Zero || finishingTime >= TimeSpan.FromDays(1))
                return BadRequest("Enter a valid finishing time.");

            int? eventID = db.getEventIDForResult(resultID);
            if (eventID == null) return NotFound("Result not found.");
            if (!db.organiserOwnsEvent(eventID.Value, userID)) return Forbid();

            string result = db.updateResult(resultID, finishingTime, finishingPosition);
            if (result == "SUCCESS") return Ok("Result updated successfully.");
            if (result == "NOT_FOUND") return NotFound("Result not found.");
            return StatusCode(500, "Could not update result.");
        }

        [HttpDelete("{resultID:int}")]
        public IActionResult DeleteResult([FromRoute] int resultID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (resultID <= 0) return BadRequest("Enter a valid result ID.");

            int? eventID = db.getEventIDForResult(resultID);
            if (eventID == null) return NotFound("Result not found.");
            if (!db.organiserOwnsEvent(eventID.Value, userID)) return Forbid();

            string result = db.deleteResult(resultID);
            if (result == "SUCCESS") return Ok("Result deleted successfully.");
            if (result == "NOT_FOUND") return NotFound("Result not found.");
            return StatusCode(500, "Could not delete result.");
        }

    }
}