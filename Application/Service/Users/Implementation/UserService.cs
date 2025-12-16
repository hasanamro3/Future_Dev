using Application.DTOs.Auth.Register;
using Application.DTOs.Student.Admin;
using Application.DTOs.Student.Student;
using Application.DTOs.Students.Admin;
using Application.Repositories.Interfaces;
using Application.Service.Students.Interface;
using Domain.Entites.Enums;
using Domain.Entites.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Service.Students.Implementation
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserService(IGenericRepository<User> userRepo, IGenericRepository<Student> studentRepo,IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _httpContextAccessor = httpContextAccessor;
    
        }
       
        public async Task DeleteStudent(int userId)
        {
            var student = await _studentRepo.GetAll().Include(s => s.User)
                .Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)  throw new InvalidOperationException("Student not found.");

            if (student.Enrollments.Any())
                throw new InvalidOperationException("Student is registered in one or more courses.");

            _studentRepo.Delete(student);
            _userRepo.Delete(student.User);

            await _userRepo.SaveChanges();
        }

        public async Task<List<StudentResponseDto>> GetAllStudents()
        {
            return await _studentRepo.GetAll()
                .Include(s => s.User)
                    .ThenInclude(u => u.Role)
                .Select(s => new StudentResponseDto
                {
                    StudentId = s.StudentId,
                    UniversityName = s.UnivercityName,
                    BirthDate = s.BirthDate,

                    UserId = s.User!.UserId,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    UserRole = s.User.Role!.RoleName
                }).ToListAsync();
        }


        public async Task<StudentResponseDto?> GetStudent(int studentId)
        {
            var student = await _studentRepo.GetAll()
                .Include(s => s.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                throw new InvalidOperationException("Student not found.");

            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                UniversityName = student.UnivercityName,
                BirthDate = student.BirthDate,

                UserId = student.User!.UserId,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                UserRole = student.User.Role!.RoleName
            };
        }


        public async Task<StudentResponseDto> StudentProfile()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated.");

            var userId = Convert.ToInt32(userIdClaim);

            var student = await _studentRepo.GetAll()
                .Include(s => s.User).ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                throw new InvalidOperationException("Student profile not found.");

            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                UniversityName = student.UnivercityName,
                BirthDate = student.BirthDate,
                UserId = student.User!.UserId,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                UserRole = student.User.Role!.RoleName
            };
        }


        public async Task<AdminProfileResponseDto> SystemAdminProfile()
        {
            var userIdClaim = _httpContextAccessor.HttpContext? .User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated.");

            var userId = Convert.ToInt32(userIdClaim);

            var admin = await _userRepo.GetAll().Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Role!.Code == RoleEnum.Admin);

            if (admin == null)
                throw new InvalidOperationException("Admin profile not found.");

            return new AdminProfileResponseDto
            {
                UserId = admin.UserId,
                FullName = admin.FullName,
                Email = admin.Email,
                PhoneNumber = admin.PhoneNumber
            };
        }


        public async Task UpdateUserProfile(StudentProfileUpdateDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var user = await _userRepo.GetAll() .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) throw new InvalidOperationException("User not found.");

            if (user.Role.Code != RoleEnum.User) throw new UnauthorizedAccessException("Access denied.");

            var student = await _studentRepo.GetAll().FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)  throw new InvalidOperationException("Student not found.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            student.UnivercityName = dto.UniversityName;
            student.BirthDate = dto.BirthDate;

            _userRepo.Update(user);
            _studentRepo.Update(student);

            await _userRepo.SaveChanges();
        }

        public async Task UpdateAdminProfile(AdminProfileUpdateDto dto)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);


            var admin = await _userRepo.GetAll().Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (admin == null) throw new InvalidOperationException("Admin not found.");

            if (admin.Role.Code != RoleEnum.Admin) throw new UnauthorizedAccessException("Access denied.");

            admin.FullName = dto.FullName;
            admin.Email = dto.Email;
            admin.PhoneNumber = dto.PhoneNumber;

            _userRepo.Update(admin);
            await _userRepo.SaveChanges();
        }

        public async Task UpdateStudentProfileByAdmin(AdminProfileResponseDto input)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated.");

            var adminId = Convert.ToInt32(userIdClaim);

            var admin = await _userRepo.GetAll().Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == adminId);

            if (admin == null || admin.Role.Code != RoleEnum.Admin)
                throw new UnauthorizedAccessException("Only admin can edit student profiles.");

            var student = await _studentRepo.GetAll().Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == input.StudentId);

            if (student == null)
                throw new InvalidOperationException("Student not found.");

            student.User!.FullName = input.FullName;
            student.User.Email = input.Email;
            student.User.PhoneNumber = input.PhoneNumber;
            student.UnivercityName = input.UniversityName;
            student.BirthDate = input.Dob;

            _studentRepo.Update(student);
            _userRepo.Update(student.User);

            await _userRepo.SaveChanges();
        }


    }

}
