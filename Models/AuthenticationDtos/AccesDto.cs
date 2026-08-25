using System;
using System.Collections.Generic;
using System.Text;

namespace Models.AuthenticationDtos
{
    public class AccesDto
    {
            public string UserId { get; set; } = string.Empty;

            public bool CanViewAllCategories { get; set; }

            public List<int> KategorijaIds { get; set; } = new();
        
    }
}
