using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth.Register
{
    public class RegisterRequestDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public DateTime DOB { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string UniName { get; set; }
        [Required]
        public string ConfirmedPassword { get; set; }
    }
}
