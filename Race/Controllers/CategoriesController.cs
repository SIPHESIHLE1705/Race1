using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/events/{eventID:int}/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly store_info db = new store_info();

        [HttpGet]
        public IActionResult GetCategories([FromRoute] int eventID)
        {
            if (db.getEventByID(eventID) == null)
                return NotFound("Event not found.");

            return Ok(db.getCategories(eventID));
        }

        [HttpPost]
        public IActionResult CreateCategory(
            [FromRoute] int eventID,
            [FromQuery] string categoryName,
            [FromQuery] int categoryAge,
            [FromQuery] decimal categoryDistance,
            [FromQuery] TimeSpan startTime)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");

            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (role != "Organiser")
                return Forbid();

            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Category name is required.");

            if (categoryName.Length > 50)
                return BadRequest("Category name is too long.");

            if (categoryAge < 0)
                return BadRequest("Age cannot be negative.");

            if (categoryDistance <= 0)
                return BadRequest("Distance must be greater than zero.");

            if (startTime < TimeSpan.Zero ||
                startTime >= TimeSpan.FromDays(1))
            {
                return BadRequest("Enter a valid start time.");
            }

            CreateCategoryRequest request = new CreateCategoryRequest
            {
                categoryName = categoryName,
                categoryAge = categoryAge,
                categoryDistance = categoryDistance,
                startTime = startTime
            };

            if (!db.createCategory(request, eventID))
                return StatusCode(500, "Could not create category.");

            return Ok("Category created successfully.");
        }

        [HttpPut("{categoryID:int}")]
        public IActionResult UpdateCategory(
            [FromRoute] int eventID,
            [FromRoute] int categoryID,
            [FromQuery] string categoryName,
            [FromQuery] int categoryAge,
            [FromQuery] decimal categoryDistance,
            [FromQuery] TimeSpan startTime)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest("Category name is required.");
            if (categoryName.Length > 50) return BadRequest("Category name is too long.");
            if (categoryAge < 0) return BadRequest("Age cannot be negative.");
            if (categoryDistance <= 0) return BadRequest("Distance must be greater than zero.");
            if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromDays(1))
                return BadRequest("Enter a valid start time.");

            var request = new CreateCategoryRequest
            {
                categoryName = categoryName,
                categoryAge = categoryAge,
                categoryDistance = categoryDistance,
                startTime = startTime
            };
            string result = db.updateCategory(eventID, categoryID, request);
            if (result == "SUCCESS") return Ok("Category updated successfully.");
            if (result == "NOT_FOUND") return NotFound("Category not found.");
            if (result == "DUPLICATE_CATEGORY")
                return Conflict("A category with that name and distance already exists for this event.");
            return StatusCode(500, "Could not update category.");
        }

        [HttpDelete("{categoryID:int}")]
        public IActionResult DeleteCategory(
            [FromRoute] int eventID,
            [FromRoute] int categoryID)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            string? role = HttpContext.Session.GetString("userRole");
            if (userID == 0) return Unauthorized("Please log in first.");
            if (role != "Organiser") return Forbid();
            if (!db.organiserOwnsEvent(eventID, userID))
                return NotFound("Event not found for this organiser.");

            string result = db.deleteCategory(eventID, categoryID);
            if (result == "SUCCESS") return Ok("Category deleted successfully.");
            if (result == "NOT_FOUND") return NotFound("Category not found.");
            if (result == "HAS_ENROLMENTS")
                return Conflict("This category has enrolments. Remove the enrolments before deleting it.");
            return StatusCode(500, "Could not delete category.");
        }

    }
}