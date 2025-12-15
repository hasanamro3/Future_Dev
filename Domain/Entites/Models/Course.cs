using System.ComponentModel.DataAnnotations;

namespace Domain.Entites.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [Required, MaxLength(500)]
        public string Description { get; set; }
        [Range(10,3000)]
        public double Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
