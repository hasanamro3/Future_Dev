
namespace Application.DTOs.Student.Admin
{
    public class EditStudentProfileDto
    {
        public int StudentId { get; set; }

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string UniversityName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
    }
}
