using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth.Login
{
    public class LoginRequestDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
