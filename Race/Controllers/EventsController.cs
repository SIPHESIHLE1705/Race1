using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly store_info db = new store_info();

        [HttpGet]
        public IActionResult GetEvents()
        {
            return Ok(db.getEvents());
        }

        // PART 3: events owned by the logged-in Organiser, with enrolment counts
        [HttpGet("mine")]
        public IActionResult GetMyEvents()
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            if (userID == 0)
                return Unauthorized("Please log in first.");
            if (HttpContext.Session.GetString("userRole") != "Organiser")
                return StatusCode(StatusCodes.Status403Forbidden, "Only organisers can do that.");

            return Ok(db.getOrganiserEvents(userID));
        }

        [HttpGet("{eventID:int}")]
        public IActionResult GetEvent([FromRoute] int eventID)
        {
            Event? item = db.getEventByID(eventID);

            if (item == null)
                return NotFound("Event not found.");

            return Ok(item);
        }

        [HttpPost]
        public IActionResult CreateEvent(
            [FromQuery] string eventName,
            [FromQuery] string eventDescription,
            [FromQuery] string eventLocation,
            [FromQuery] decimal eventDistance,
            [FromQuery] DateTime eventDate,
            [FromQuery] string eventType)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (role != "Organiser")
                return Forbid();

            if (string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(eventDescription) ||
                string.IsNullOrWhiteSpace(eventLocation))
            {
                return BadRequest("All event fields are required.");
            }

            if (eventName.Length > 100 ||
                eventDescription.Length > 500 ||
                eventLocation.Length > 100)
            {
                return BadRequest("One or more fields are too long.");
            }

            if (eventDistance <= 0)
                return BadRequest("Distance must be greater than zero.");

            if (eventDate == default)
                return BadRequest("A valid event date is required.");

            if (eventType != "Run" &&
                eventType != "Walk" &&
                eventType != "Cycle")
            {
                return BadRequest("Event type must be Run, Walk or Cycle.");
            }

            CreateEventRequest request = new CreateEventRequest
            {
                eventName = eventName,
                eventDescription = eventDescription,
                eventLocation = eventLocation,
                eventDistance = eventDistance,
                eventDate = eventDate,
                eventType = eventType
            };

            if (!db.createEvent(request, userID))
                return StatusCode(500, "Could not create event.");

            return Ok("Event created successfully.");
        }

        [HttpPut("{eventID:int}")]
        public IActionResult UpdateEvent(
            [FromRoute] int eventID,
            [FromQuery] string eventName,
            [FromQuery] string eventDescription,
            [FromQuery] string eventLocation,
            [FromQuery] decimal eventDistance,
            [FromQuery] DateTime eventDate,
            [FromQuery] string eventType)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            if (string.IsNullOrWhiteSpace(eventName) ||
                string.IsNullOrWhiteSpace(eventDescription) ||
                string.IsNullOrWhiteSpace(eventLocation))
                return BadRequest("All event fields are required.");
            if (eventName.Length > 100 || eventDescription.Length > 500 ||
                eventLocation.Length > 100)
                return BadRequest("One or more fields are too long.");
            if (eventDistance <= 0) return BadRequest("Distance must be greater than zero.");
            if (eventDate == default) return BadRequest("A valid event date is required.");
            if (eventType != "Run" && eventType != "Walk" && eventType != "Cycle")
                return BadRequest("Event type must be Run, Walk or Cycle.");

            var request = new CreateEventRequest
            {
                eventName = eventName,
                eventDescription = eventDescription,
                eventLocation = eventLocation,
                eventDistance = eventDistance,
                eventDate = eventDate,
                eventType = eventType
            };
            string result = db.updateEvent(eventID, userID, request);
            if (result == "SUCCESS") return Ok("Event updated successfully.");
            if (result == "NOT_FOUND") return NotFound("Event not found.");
            return StatusCode(500, "Could not update event.");
        }

        [HttpDelete("{eventID:int}")]
        public IActionResult DeleteEvent([FromRoute] int eventID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            string result = db.deleteEvent(eventID, userID);
            if (result == "SUCCESS") return Ok("Event deleted successfully.");
            if (result == "HAS_DEPENDENCIES")
                return Conflict("Delete dependent results, enrolments and categories before deleting the event.");
            if (result == "NOT_FOUND") return NotFound("Event not found.");
            return StatusCode(500, "Could not delete event.");
        }

    }
}