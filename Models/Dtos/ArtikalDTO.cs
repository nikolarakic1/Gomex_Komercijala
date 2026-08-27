using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dtos
{
    public class ArtikalDto
    {
        public int ArtikalId { get; set; }

        public string Sifra { get; set; } = string.Empty;
        
        public string Naziv { get; set; } = string.Empty;

        public int DobavljacId { get; set; }

        public int RobnaGrupaId { get; set; }

        public bool Aktivan { get; set; }
        public decimal? RedovnaCena { get; set; }
    }
}
