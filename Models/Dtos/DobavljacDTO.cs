using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dtos
{
    public class DobavljacDTO
    {
        public int DobavljacId { get; set; }

        public string Naziv { get; set; } = string.Empty;

        public bool Aktivan { get; set; }
    }
}
