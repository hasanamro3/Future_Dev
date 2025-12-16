
namespace Application.DTOs.Student.Admin
{
    public class AdminProfileResponseDto
    {
        //for Admin data
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        //for student data

        public int StudentId { get; set; }
        public DateTime Dob { get; set; }
        public string UniversityName { get; internal set; }
    }
}
