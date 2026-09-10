using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
   
        public class RegisterStudentResponseDto
        {
            public Guid UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Message { get; set; } = "Student account failed to be created.";
        public string UniversityNumber { get; set; } = string.Empty;
    }
}

