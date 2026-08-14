using System;
using System.Collections.Generic;
using System.Text;

namespace Models.AuthenticationDtos
{
    public class RegisterDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
