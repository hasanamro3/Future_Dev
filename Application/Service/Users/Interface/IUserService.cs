using Application.DTOs.Student.Admin;
using Application.DTOs.Student.Student;
using Application.DTOs.Students.Admin;

namespace Application.Service.Students.Interface
{
    public interface IUserService
    {
        Task<AdminProfileResponseDto> SystemAdminProfile();
        Task<StudentResponseDto> StudentProfile();
        Task UpdateUserProfile(StudentProfileUpdateDto dto);
        Task UpdateAdminProfile(AdminProfileUpdateDto dto);
        Task DeleteStudent(int id);
        Task<StudentResponseDto?> GetStudent(int id);
        Task<List<StudentResponseDto>> GetAllStudents();
        Task UpdateStudentProfileByAdmin(AdminProfileResponseDto input);
    }
}
