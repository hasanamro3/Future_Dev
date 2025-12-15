using Application.DTOs.Courses;

namespace Application.Service.Courses.Interface
{
    public interface ICourseService
    {
        Task CreateCourse(CreateCourseDto dto);
        Task UpdateCourse(int courseId, UpdateCourseDto dto);
        Task DeleteCourse(int courseId);
        Task<CourseResponseDto?> GetCourseById(int courseId);
        Task<List<CourseResponseDto>> GetAllCourses();
        Task<List<CourseResponseDto>> SearchCourses(string title);
    }

}
