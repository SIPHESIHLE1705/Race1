namespace Race.MVC.Models
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
