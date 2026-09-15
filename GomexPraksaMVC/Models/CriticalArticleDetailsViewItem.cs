namespace GomexPraksaMVC.Models
{
    public class CriticalArticleDetailsViewItem
    {
        public int ArtikalId { get; set; }

        public string Sifra { get; set; } =
            string.Empty;

        public string Naziv { get; set; } =
            string.Empty;

        public string? Dobavljac { get; set; }

        public string TrenutniStatus { get; set; } =
            "REDOVNA";

        public string? TrenutniTipAkcije { get; set; }

        public decimal? RedovnaCena { get; set; }

        public decimal? AkcijskaCena { get; set; }

        public List<CriticalArticlePerformanceViewItem>
            Performanse
        { get; set; } =
                new();

        public decimal MarginEffect { get; set; }

        public decimal VolumeEffect { get; set; }

        public decimal MixEffect { get; set; }

        public decimal TotalDiff { get; set; }
    }

    public class CriticalArticlePerformanceViewItem
    {
        public int Godina { get; set; }

        public string Tip { get; set; } =
            string.Empty;

        public string VrstaProdaje { get; set; } =
            string.Empty;

        public string TipAkcije { get; set; } =
            string.Empty;

        public decimal Kolicina { get; set; }

        public decimal Promet { get; set; }

        public decimal Ruc12 { get; set; }

        public decimal Ruc12Procenat { get; set; }

        public bool CmUtice { get; set; }
    }
}