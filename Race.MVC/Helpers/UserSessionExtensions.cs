using Race.MVC.Models;

namespace Race.MVC.Helpers
{
    public static class UserSessionExtensions
    {
        public static bool IsLoggedIn(this ISession session)
            => session.GetInt32(SessionKeys.UserId).HasValue;

        public static string? GetRole(this ISession session)
            => session.GetString(SessionKeys.UserRole);

        public static string GetFullName(this ISession session)
            => session.GetString(SessionKeys.FullName) ?? string.Empty;

        public static void SignIn(this ISession session, LoginResponseDto user)
        {
            session.SetInt32(SessionKeys.UserId, user.UserID);
            session.SetString(SessionKeys.UserRole, user.UserRole);
            session.SetString(SessionKeys.FullName, user.FullName);
        }
    }
}
