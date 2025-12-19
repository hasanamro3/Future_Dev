using Application.DTOs.Courses;
using Application.Service.Courses.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Future_Dev.Controllers
{

    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("GetAllCourses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _courseService.GetAllCourses());
        }

        [HttpGet("GetCourse/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetCourseById(id);

            if (course == null) return NotFound("Course not found.");

            return Ok(course);
        }

        [HttpPost("CreateCourse")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            await _courseService.CreateCourse(dto);
            return Ok("Course created successfully.");
        }

        [HttpPut("UpdateCourse/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            await _courseService.UpdateCourse(id, dto);
            return Ok("Course updated successfully.");
        }

        [HttpDelete("DeleteCourse/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid("Only Admins can delete students");

            await _courseService.DeleteCourse(id);
            return Ok("Course deleted successfully.");
        }

        [HttpGet("SearchCourse")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Search([FromQuery] string title)
        {
            return Ok(await _courseService.SearchCourses(title));
        }

        [Authorize(Roles = "User")]
        [HttpGet("MyCourses")]
        public async Task<IActionResult> GetMyCourses()
        {
            return Ok(await _courseService.GetMyCourses());
        }

        [HttpGet("GetCourseByUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCoursesByStudentId([FromQuery] int studentId)
        {
            return Ok(await _courseService.GetCoursesByStudentId(studentId));
        }

        [HttpGet("GetUsersByCourse")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentsByCourseId([FromQuery] int courseId)
        {
            return Ok(await _courseService.GetStudentsByCourseId(courseId));
        }

    }

}
