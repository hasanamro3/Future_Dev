using Application.DTOs.Enrollment;
using Application.Service.Enrollments.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Future_Dev.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpPost("CreateEnrollment")]
        public async Task<IActionResult> Create(CreateEnrollmentByAdminDto dto)
        {
            await _enrollmentService.CreateEnrollment(dto);
            return Ok("Enrollment created.");
        }

        [HttpGet("GetAllEnrollments")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _enrollmentService.GetAllEnrollments());
        }

        [HttpGet("GetByStudent/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            return Ok(await _enrollmentService.GetEnrollmentsByStudent(studentId));
        }

        [HttpDelete("DeleteEnrollment")]
        public async Task<IActionResult> Delete(int studentId, int courseId)
        {
            await _enrollmentService.DeleteEnrollment(studentId, courseId);
            return Ok("Enrollment deleted.");
        }
    }

}
