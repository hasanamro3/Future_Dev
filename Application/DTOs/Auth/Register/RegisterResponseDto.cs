
using Domain.Entites.Models;

namespace Application.DTOs.Auth.Register
{
    public class RegisterResponseDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int RoleId { get; set; }
    }
}
