namespace Race.MVC.Helpers
{
    // Stores the names of values kept in the MVC session.
    public static class SessionKeys
    {
        // Stores the logged-in user's ID.
        public const string UserId = "userID";

        // Stores the logged-in user's role.
        public const string UserRole = "userRole";

        // Stores the logged-in user's full name.
        public const string FullName = "fullName";

        // Stores the API session cookie used to maintain the user's API session.
        public const string ApiCookie = "apiCookie";
    }
}
