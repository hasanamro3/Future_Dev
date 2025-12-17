
namespace Application.DTOs.Auth.Login
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
