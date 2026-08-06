namespace GomexPraksaMVC.Models
{
    public class DashboardViewModel
    {
        public decimal PrometBezPdv { get; set; }
        public decimal PrometPromenaProcenat { get; set; }

        public decimal Ruc12 { get; set; }
        public decimal Ruc12PromenaProcenat { get; set; }

        public decimal Ruc12Procenat { get; set; }
        public decimal Ruc12PromenaProcentniPoeni { get; set; }

        public int KriticniArtikli { get; set; }
        public int KriticniArtikliPromena { get; set; }

        public decimal NedostatakMarze { get; set; }
        public decimal NedostatakMarzePromenaProcenat { get; set; }

        public DateTime? PodaciOsvezeni { get; set; }

        public List<DobavljacViewItem> Dobavljaci { get; set; } = new();
        public int? SelectedDobavljacId { get; set; }
    }

    public class DobavljacViewItem
    {
        public int DobavljacId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool Aktivan { get; set; }
    }
}