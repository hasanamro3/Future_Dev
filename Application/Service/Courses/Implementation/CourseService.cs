namespace Application.Service.Courses.Implementation
{
    using Application.DTOs.Category;
    using Application.DTOs.Courses;
    using Application.DTOs.Students.Student;
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

        public CourseService(IGenericRepository<Course> courseRepo, IGenericRepository<Category> categoryRepo, IGenericRepository<User> userRepo, IHttpContextAccessor httpContextAccessor)
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

            if (category == null) throw new InvalidOperationException("Category not found.");

            if (dto.StartDate > dto.EndDate)

                throw new InvalidOperationException("Invalid Dates.");

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                CategoryId = dto.CategoryId
            };

            await _courseRepo.Insert(course);
            await _courseRepo.SaveChanges();
        }

        public async Task UpdateCourse(int courseId, UpdateCourseDto dto)
        {
            await IsAdmin();

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var course = await _courseRepo.GetById(courseId);

            if (course == null)
                throw new InvalidOperationException("Course not found.");

            if (course.StartDate <= DateTime.UtcNow)
                throw new InvalidOperationException("Cannot update a course that has already started.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("Course title is required.");

            if (dto.Price < 0)
                throw new InvalidOperationException("Course price cannot be negative.");

            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("End date must be after start date.");


            course.Title = dto.Title.Trim();
            course.Description = dto.Description!.Trim();
            course.Price = dto.Price;
            course.StartDate = dto.StartDate;
            course.EndDate = dto.EndDate;

            _courseRepo.Update(course);
            await _courseRepo.SaveChanges();
        }

        public async Task DeleteCourse(int courseId)
        {
            await IsAdmin();

            var course = await _courseRepo.GetById(courseId);

            if (course == null) throw new InvalidOperationException("Course not found.");

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

        public async Task<List<CourseResponseDto>> GetCoursesByStudentId(int studentId)
        {
            await IsAdmin();

            var courses = await _courseRepo.GetAll()
                .Include(c => c.Category).Include(c => c.Enrollments)
                .Where(c => c.Enrollments.Any(e => e.StudentId == studentId)).ToListAsync();

            return courses.Select(course => new CourseResponseDto
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CategoryName = course.Category!.Name
            }).ToList();
        }

        public async Task<List<CourseResponseDto>?> GetMyCourses()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null) return null;

            var userId = Convert.ToInt32(userIdClaim);

            var courses = await _courseRepo.GetAll()
             .Include(c => c.Category)
             .Where(c => c.Enrollments.Any(e => e.StudentId == userId))
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

            if (courses == null) return null;

            return courses;
        }

        public async Task<List<CourseResponseDto>> GetAllCourses()
        {

            return await _courseRepo.GetAll().Include(c => c.Category).Where(c => c.StartDate >= DateTime.UtcNow)

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
                .Where(c => c.Title.ToLower().Trim() == title.ToLower().Trim())
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

        public async Task<List<StudentResponseDto>?> GetStudentsByCourseId(int courseId)
        {
            var hasEnrollments = await _courseRepo.GetAll()
                .AnyAsync(c =>c.CourseId == courseId && c.Enrollments.Any());

            if (!hasEnrollments)  return null;


            var students = await _courseRepo.GetAll()
                   .Where(c => c.CourseId == courseId).Include(c => c.Enrollments)
                   .ThenInclude(e => e.Student).ThenInclude(s => s!.User)
                   .Select(c => c.Enrollments
                       .Select(e => new StudentResponseDto
                       {
                           StudentId = e.Student!.StudentId,
                           FullName = e.Student.User!.FullName,
                           UniversityName = e.Student.UnivercityName
                       }).ToList()
                    ).FirstOrDefaultAsync();


            return students!.Any() ? students : null;
        }

        public async Task<List<CategoriesDto>> GetAllCategories()
        {
            var categories = await _categoryRepo.GetAll().ToListAsync();

            var categoriesDto = categories.Select(c => new CategoriesDto
            {
                CategoryName = c.Name
            }).ToList();

            return categoriesDto;
        }

    }
}
