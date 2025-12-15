using System.ComponentModel.DataAnnotations;

namespace Domain.Entites.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        [Required]
        public string UnivercityName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreateAt { get; set; }= DateTime.UtcNow;

        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
