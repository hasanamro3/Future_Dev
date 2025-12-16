using Application.DTOs.Auth.Register;
using Application.DTOs.Student.Admin;
using Application.DTOs.Student.Student;
using Application.DTOs.Students.Admin;
using Domain.Entites.Models;

namespace Application.Service.Students.Interface
{
    public interface IUserService
    {
        Task<User> SystemAdminProfile();
        Task<Student> StudentProfile();
        Task UpdateUserProfile(StudentProfileUpdateDto dto);
        Task UpdateAdminProfile(AdminProfileUpdateDto dto);
        Task DeleteStudent(int id);
        Task<Student?> GetStudent(int id);
        Task<List<Student>> GetAllStudents();
        Task UpdateStudentProfileByAdmin(EditStudentProfileDto input);
    }
}
