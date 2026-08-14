using System;
using System.Collections.Generic;
using System.Text;

namespace Models.AuthenticationDtos
{
    public class LogInDto
    {
        public string Email { get; set; } = string.Empty;
        public string Passsword { get; set; } = string.Empty;
    }
}
