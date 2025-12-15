using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service.Courses.Implementation
{
    using Application.DTOs.Courses;
    using Application.Repositories.Interfaces;
    using Application.Service.Courses.Interface;
    using Domain.Entites.Enums;
    using Domain.Entites.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;

    public class CourseService : ICourseService
    {
        private readonly IGenericRepository<Course> _courseRepo;
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseService(IGenericRepository<Course> courseRepo,IGenericRepository<Category> categoryRepo,IGenericRepository<User> userRepo,IHttpContextAccessor httpContextAccessor)
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
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

        public async Task CreateCourse(CreateCourseDto dto)
        {
            await IsAdmin();

            var category = await _categoryRepo.GetById(dto.CategoryId);

            if (category == null)  throw new InvalidOperationException("Category not found.");

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CategoryId = dto.CategoryId
            };

            _courseRepo.Insert(course);
            await _courseRepo.SaveChanges();
        }

        public async Task UpdateCourse(int courseId, UpdateCourseDto dto)
        {
            await IsAdmin();

            var course = await _courseRepo.GetById(courseId);

            if (course == null)  throw new InvalidOperationException("Course not found.");

            // ❌ لا تعديل بعد بدء الكورس
            if (course.StartDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot update a course that has already started.");

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.Price = dto.Price;
            course.EndDate = dto.EndDate;

            _courseRepo.Update(course);
            await _courseRepo.SaveChanges();
        }

        public async Task DeleteCourse(int courseId)
        {
            await IsAdmin();

            var course = await _courseRepo.GetById(courseId);

            if (course == null)  throw new InvalidOperationException("Course not found.");

            _courseRepo.Delete(course);
            await _courseRepo.SaveChanges();
        }

        public async Task<CourseResponseDto?> GetCourseById(int courseId)
        {
            await IsAdmin();
            var course = await _courseRepo.GetAll()
                .Include(c => c.Category).FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null) return null;

            return new CourseResponseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CategoryName = course.Category!.Name
            };
        }

        public async Task<List<CourseResponseDto>> GetAllCourses()
        {
            await IsAdmin();
            return await _courseRepo.GetAll().Include(c => c.Category)
                .Select(c => new CourseResponseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CategoryName = c.Category!.Name
                }).ToListAsync();
        }

        public async Task<List<CourseResponseDto>> SearchCourses(string title)
        {
            return await _courseRepo.GetAll().Include(c => c.Category)
                .Where(c => c.Title.ToLower().Trim()==title.ToLower().Trim())
                .Select(c => new CourseResponseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    CategoryName = c.Category!.Name
                }).ToListAsync();
        }

    }

}
