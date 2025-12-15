using Application.DTOs.Enrollment;

namespace Application.Service.Enrollments.Interface
{
    public interface IEnrollmentService
    {
        Task CreateEnrollment(CreateEnrollmentByAdminDto dto);
        Task DeleteEnrollment(int studentId, int courseId);
        Task<List<EnrollmentResponseDto>> GetAllEnrollments();
        Task<List<EnrollmentResponseDto>> GetEnrollmentsByStudent(int studentId);
    }
}
