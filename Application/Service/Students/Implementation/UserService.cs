using Application.DTOs.Auth.Register;
using Application.DTOs.Student.Admin;
using Application.DTOs.Student.Student;
using Application.DTOs.Students.Admin;
using Application.Repositories.Interfaces;
using Application.Service.Students.Interface;
using Domain.Entites.Enums;
using Domain.Entites.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Service.Students.Implementation
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<Student> _studentRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepo;


        public UserService(IGenericRepository<User> userRepo, IGenericRepository<Student> studentRepo,IHttpContextAccessor httpContextAccessor, IGenericRepository<RefreshToken> refreshTokenRep, IConfiguration config)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _refreshTokenRepo = refreshTokenRep;
        }

        public async Task<RegisterResponseDto> CreateStudent(CreateStudentRequestDto dto)
        {
            var existingUser = await _userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.Email!.Trim().ToLower() == dto.Email.Trim().ToLower());

            if (existingUser != null)

                throw new InvalidOperationException("A user with this email already exists.");

            var passwordHasher = new PasswordHasher<User>();

            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2
            };
          
            newUser.Password = passwordHasher.HashPassword(newUser, dto.Password);

            await _userRepo.Insert(newUser);
            await _userRepo.SaveChanges();

            var newStudent = new Student
            {
                UserId = newUser.UserId,
                BirthDate = dto.BirthDate,
                UnivercityName = dto.UniversityName,
            };


            await _studentRepo.Insert(newStudent);
            await _userRepo.SaveChanges();

            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepo.Insert(new RefreshToken
            {
                Token = refreshToken,
                UserId = newUser.UserId,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            await _refreshTokenRepo.SaveChanges();

            return new RegisterResponseDto
            {
                UserId = newUser.UserId,
                FullName = newUser.FullName,
                Email = newUser.Email,
                RoleId = newUser.RoleId,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
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

        public async Task<List<Student>> GetAllStudents()
        {
            return await _studentRepo.GetAll().Include(s => s.User).ToListAsync();
        }

        public async Task<Student?> GetStudent(int studentId)
        {
            var student=  await _studentRepo.GetAll().Include(s => s.User).Include(s => s.Enrollments)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if(student == null)  throw new InvalidOperationException("Student not found."); 

            return student;
        }

        public async Task<Student> StudentProfile()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var student = await _studentRepo.GetAll().Include(s => s.User)
                .Include(s => s.Enrollments).FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)  throw new InvalidOperationException("Student profile not found.");

            return student;
        }

        public async Task<User> SystemAdminProfile()
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var admin = await _userRepo.GetAll().Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.Role.Code == RoleEnum.Admin);

            if (admin == null) throw new InvalidOperationException("Admin profile not found.");

            return admin;
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

        public async Task UpdateStudentProfileByAdmin(EditStudentProfileDto input)
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

            student.User.FullName = input.FullName;
            student.User.Email = input.Email;
            student.User.PhoneNumber = input.PhoneNumber;
            student.UnivercityName = input.UniversityName;
            student.BirthDate = input.BirthDate;

            _studentRepo.Update(student);
            _userRepo.Update(student.User);

            await _userRepo.SaveChanges();
        }

        // Helper methods to generate tokens (implementation depends on your requirements)
        public string GenerateAccessToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
            };



            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var random = new byte[64];
            RandomNumberGenerator.Fill(random);
            return Convert.ToBase64String(random);
        }
        public async Task<string> RefreshToken(string refreshToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = Convert.ToInt32(userIdClaim);

            var storedToken = _refreshTokenRepo.GetAll()
                .FirstOrDefault(rt => rt.UserId == userId && rt.Token == refreshToken && rt.Expires > DateTime.UtcNow);
            if (storedToken == null)
            {
                throw new SecurityTokenException("Invalid refresh token.");
            }
            var user = await _userRepo.GetById(storedToken.UserId);
            return GenerateAccessToken(user);
        }

    }

}
