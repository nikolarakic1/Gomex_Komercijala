namespace GomexPraksa.KomercijalaStaging
{
    public class KomercijalaStagingRow
    {
        public string? Tip { get; set; }
        public int? Godina { get; set; }
        public int? Mesec { get; set; }
        public int? Nedelja { get; set; }

        public string? IdKampanja1 { get; set; }
        public string? IdKampanja2 { get; set; }
        public string? CmUtice { get; set; }

        public string? Odeljenje { get; set; }
        public string? Kategorija { get; set; }
        public string? RobnaGrupa { get; set; }

        public string? Sifra { get; set; }
        public string? Artikal { get; set; }

        public decimal? Pdv { get; set; }

        public string? Dobavljac { get; set; }

        public decimal? Kolicina { get; set; }
        public decimal? MpVrednost { get; set; }
        public decimal? MpBezPdv { get; set; }

        public decimal? Ruc1 { get; set; }
        public decimal? Ruc2 { get; set; }
        public decimal? Ruc12 { get; set; }

        public decimal? NabavnaVrednost { get; set; }
    }
}
