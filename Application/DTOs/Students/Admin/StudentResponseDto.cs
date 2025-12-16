using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Students.Admin
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
