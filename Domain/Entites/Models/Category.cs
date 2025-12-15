using System.ComponentModel.DataAnnotations;

namespace Domain.Entites.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required,StringLength(50,MinimumLength =2)]
        public string Name { get; set; }

    }
}
