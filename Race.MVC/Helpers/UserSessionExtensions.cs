using Race.MVC.Models;

namespace Race.MVC.Helpers
{
    // Provides extension methods for managing user session information.
    public static class UserSessionExtensions
    {
        // Checks whether a user is logged in by checking if their user ID exists in the session.
        public static bool IsLoggedIn(this ISession session)
            => session.GetInt32(SessionKeys.UserId).HasValue;

        // Retrieves the logged-in user's role from the session.
        public static string? GetRole(this ISession session)
            => session.GetString(SessionKeys.UserRole);

        // Retrieves the logged-in user's full name.
        // Returns an empty string if the full name is not stored in the session.
        public static string GetFullName(this ISession session)
            => session.GetString(SessionKeys.FullName) ?? string.Empty;

        // Stores the user's login information in the session after successful authentication.
        public static void SignIn(this ISession session, LoginResponseDto user)
        {
            // Saves the user's ID in the session.
            session.SetInt32(SessionKeys.UserId, user.UserID);

            // Saves the user's role in the session.
            session.SetString(SessionKeys.UserRole, user.UserRole);

            // Saves the user's full name in the session.
            session.SetString(SessionKeys.FullName, user.FullName);
        }
    }
}
