namespace Race.MVC.Models
{
    public class LoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
