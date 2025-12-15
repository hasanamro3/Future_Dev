using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth.Password
{
    public class ResetPasswordDto
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
