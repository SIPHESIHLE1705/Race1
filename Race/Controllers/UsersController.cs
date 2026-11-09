using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race.Database;
using Race.Models;

namespace Race.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly store_info db = new store_info();

        [HttpPost("register")]
        public IActionResult Register(
            [FromQuery] string fullname,
            [FromQuery] string email,
            [FromQuery] string password,
            [FromQuery] string role)
        {
            if (string.IsNullOrWhiteSpace(fullname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(role))
            {
                return BadRequest("All fields are required.");
            }

            if (password.Length < 8)
                return BadRequest("Password must contain at least 8 characters.");

            if (fullname.Length > 100 || email.Length > 100)
                return BadRequest("Name or email is too long.");

            if (role != "Participant" && role != "Organiser")
                return BadRequest("Role must be Participant or Organiser.");

            string result = db.createUser(fullname, email, role, password);

            if (result == "EMAIL_EXISTS")
                return Conflict("Email already registered.");

            if (result == "SUCCESS")
                return Ok("Registration successful.");

            return StatusCode(500, "Registration failed.");
        }

        [HttpGet("me")]
        public IActionResult GetMyProfile()
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;

            if (userID == 0)
                return Unauthorized("Please log in first.");

            User? user = db.getUserByID(userID);

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        [HttpPut("me")]
        public IActionResult UpdateMyProfile(
            [FromQuery] string fullName,
            [FromQuery] string emailAddress)
        {
            int userID = HttpContext.Session.GetInt32("userID") ?? 0;
            if (userID == 0)
                return Unauthorized("Please log in first.");

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(emailAddress))
                return BadRequest("Name and email are required.");

            if (fullName.Length > 100 || emailAddress.Length > 100)
                return BadRequest("Name or email is too long.");

            string result = db.updateUser(userID, fullName, emailAddress);
            if (result == "EMAIL_EXISTS")
                return Conflict("Email already registered.");
            if (result == "NOT_FOUND")
                return NotFound("User not found.");
            if (result == "SUCCESS")
                return Ok("Profile updated successfully.");
            return StatusCode(500, "Could not update profile.");
        }

    }
}