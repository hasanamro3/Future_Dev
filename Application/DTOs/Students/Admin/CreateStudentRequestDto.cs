namespace Application.DTOs.Students.Admin
{
    public class CreateStudentRequestDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string UniversityName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
    }
}
