
namespace Application.DTOs.Student.Student
{
    public class StudentProfileUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string UniversityName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
    }
}
