using System;
using System.Collections.Generic;
using System.Text;

namespace Models.DtosComerc
{
    public class CriticalProductsPageDTO
    {
        public int ArtikalId { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public string Naziv { get; set; } = string.Empty;
        public string? Dobavljac { get; set; }

        public decimal Promet { get; set; }
        public decimal PlanPrometa { get; set; }
        public decimal RUC12 { get; set; }
        public decimal PlanRuc12 { get; set; }
        public decimal RUC12Procenat { get; set; }
        public decimal PlanRucProcenat { get; set; }
        public decimal OdstupanjeProcentaPoeni { get; set; }
        public decimal NedostatakMargine { get; set; }
        public decimal ProcenjeniUticaj { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
