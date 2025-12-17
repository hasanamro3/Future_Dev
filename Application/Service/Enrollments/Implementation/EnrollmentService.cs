using Application.DTOs.Enrollment;
using Application.Repositories.Interfaces;
using Application.Service.Enrollments.Interface;
using Domain.Entites.Enums;
using Domain.Entites.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Service.Enrollments.Implementation
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IGenericRepository<Enrollment> _enrollmentRepo;
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EnrollmentService(IGenericRepository<Enrollment> enrollmentRepo,IGenericRepository<Student> studentRepo,IGenericRepository<Course> courseRepo,IGenericRepository<User> userRepo,IHttpContextAccessor httpContextAccessor)
        {
            _enrollmentRepo = enrollmentRepo;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _userRepo = userRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task IsAdmin()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var userId = Convert.ToInt32(userIdClaim);

            var Admin = await _userRepo.GetAll().Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Role!.Code == RoleEnum.Admin);

            if (Admin == null)
                throw new UnauthorizedAccessException("Only system admin can manage enrollments.");
        }

        public async Task CreateEnrollment(CreateEnrollmentByAdminDto dto)
        {
            await IsAdmin();

            var student = await _studentRepo.GetAll().FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);

            if (student == null) throw new InvalidOperationException("Student not found.");

            var course = await _courseRepo.GetById(dto.CourseId);

            if (course == null) throw new InvalidOperationException("Course not found.");

            var exists = await _enrollmentRepo.GetAll().AnyAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);

            if (exists) throw new InvalidOperationException("Student already enrolled in this course.");

            if (course.StartDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot enroll in a course that has already started.");
            if(course.EndDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot enroll in a course that has already ended.");
            if(dto.CreatedAt<= DateTime.UtcNow)
                throw new InvalidOperationException("Enrollment date must be in the future.");
            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                EnrollmentDate= dto.CreatedAt
            };

            await _enrollmentRepo.Insert(enrollment);
            await _enrollmentRepo.SaveChanges();
        }

        public async Task DeleteEnrollment(int studentId, int courseId)
        {
            await IsAdmin();

            var enrollment = await _enrollmentRepo.GetAll().Include(e => e.Student)
                    .ThenInclude(s => s!.User) .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
                throw new InvalidOperationException("Enrollment not found.");

 
            await _enrollmentRepo.Delete(enrollment);
            await _enrollmentRepo.SaveChanges();
        }

        public async Task<List<EnrollmentResponseDto>> GetAllEnrollments()
        {
            await IsAdmin();

            return await _enrollmentRepo.GetAll().Include(s => s.Student).ThenInclude(u => u!.User).Include(c => c.Course)
                .Select(e => new EnrollmentResponseDto
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FullName,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course!.Title
                }).ToListAsync();
        }

        public async Task<List<EnrollmentResponseDto>> GetEnrollmentsByStudent(int studentId)
        {
            await IsAdmin();

            return await _enrollmentRepo.GetAll()
                .Include(e => e.Student).ThenInclude(s => s!.User)
                .Include(e => e.Course).Where(e => e.StudentId == studentId)
                .Select(e => new EnrollmentResponseDto
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FullName,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course!.Title
                }).ToListAsync();
        }

        public async Task<List<EnrollmentResponseDto>> GetEnrollmentsByCourse(int courseId)
        {
            await IsAdmin();

            return await _enrollmentRepo.GetAll()
                .Include(e => e.Student).ThenInclude(s => s!.User)
                .Include(e => e.Course).Where(e => e.CourseId == courseId)
                .Select(e => new EnrollmentResponseDto
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student!.User!.FullName,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course!.Title
                }).ToListAsync();
        }
    }
}
