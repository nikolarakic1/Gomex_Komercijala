using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dtos
{
    public class ArtikalFilterDto
    {
        public string? Naziv { get; set; }

        public int? DobavljacId { get; set; }

        public int? RobnaGrupaId { get; set; }

        public bool? Aktivan { get; set; }
    }
}
