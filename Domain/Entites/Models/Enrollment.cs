using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entites.Models
{
    public class Enrollment
    {
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course{ get; set; }
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
