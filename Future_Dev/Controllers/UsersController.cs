using Application.DTOs;
using Application.DTOs.Student.Admin;
using Application.DTOs.Student.Student;
using Application.DTOs.Students.Admin;
using Application.Service.Students.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

  
    [Authorize(Roles = "Admin")]
    [HttpGet("GetAllStudents")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _userService.GetAllStudents();
        return Ok(students);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetStudent/{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _userService.GetStudent(id);

        if (student == null)
            return NotFound("Student not found.");

        return Ok(student);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("updateAdminProfile")]
    public async Task<IActionResult> UpdateAdminProfile([FromBody] AdminProfileUpdateDto dto)
    {
        await _userService.UpdateAdminProfile(dto);
        return Ok("Admin profile updated successfully.");
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("DeleteStudent/{userId}")]
    public async Task<IActionResult> DeleteStudent(int userId)
    {
        await _userService.DeleteStudent(userId);
        return Ok("Student deleted successfully.");
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("updateStudentProfile/{id}")]
    public async Task<IActionResult> UpdateStudentProfileByAdmin(int id, [FromBody] AdminProfileResponseDto dto)
    {
        dto.StudentId = id;
        await _userService.UpdateStudentProfileByAdmin(dto);
        return Ok("Profile updated successfully.");
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("SystemAdminProfile")]
    public async Task<IActionResult> SystemAdminProfile()
    {
        var student = await _userService.SystemAdminProfile();
        return Ok(student);
    }

    [Authorize(Roles = "User")]
    [HttpGet("GetStudentProfile")]
    public async Task<IActionResult> GetStudentProfile()
    {
        var student = await _userService.StudentProfile();
        return Ok(student);
    }

    [Authorize(Roles = "User")]
    [HttpPut("updateStudentProfile")]
    public async Task<IActionResult> UpdateStudentProfile([FromBody] StudentProfileUpdateDto dto)
    {
        await _userService.UpdateUserProfile(dto);
        return Ok("Profile updated successfully.");
    }
   
}
