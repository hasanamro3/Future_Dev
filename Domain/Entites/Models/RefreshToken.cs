using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites.Models
{
    public class RefreshToken
    {
        [Key]
        public int RefreshTokenId { get; set; }
        public string? Token { get; set; }
        public DateTime Expires { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
