using System;
using System.Collections.Generic;

namespace Application.DTOs.Students.Student
{
    public class StudentResponseDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string UniversityName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int UserId { get; internal set; }
        public string UserRole { get; internal set; }
    }
}
