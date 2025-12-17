
namespace Application.DTOs.Enrollment
{
    public class CreateEnrollmentByAdminDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
