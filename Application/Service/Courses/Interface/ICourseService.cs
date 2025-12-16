using Application.DTOs.Courses;
using Application.DTOs.Students.Admin;

namespace Application.Service.Courses.Interface
{
    public interface ICourseService
    {
        Task CreateCourse(CreateCourseDto dto);
        Task UpdateCourse(int courseId, UpdateCourseDto dto);
        Task DeleteCourse(int courseId);
        Task<CourseResponseDto?> GetCourseById(int courseId);
        Task<List<CourseResponseDto>> GetCoursesByStudentId(int studentId);
        Task<List<CourseResponseDto>?> GetMyCourses();
        Task<List<CourseResponseDto>> GetAllCourses();
        Task<List<CourseResponseDto>> SearchCourses(string title);
        Task<List<StudentResponseDto>?> GetStudentsByCourseId(int courseId);
    }

}
