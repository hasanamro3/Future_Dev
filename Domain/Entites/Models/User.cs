using System.ComponentModel.DataAnnotations;

namespace Domain.Entites.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required,StringLength(50,MinimumLength =3)]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,24}$",ErrorMessage = "Invalid Password.")]
        public string Password{ get; set; }
        [Required]
        [RegularExpression(@"^(?:\+962|00962|0)7[789]\d{7}$", ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }
        [Required]
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public Student? Student { get; set; }

    }
}
