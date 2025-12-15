using Domain.Entites.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entites.Models
{
    public class Role
    {
          [Key]
          public int RoleId { get; set; }
          [Required]
          public string RoleName { get; set; }
          public RoleEnum Code { get; set; }
    }

}
