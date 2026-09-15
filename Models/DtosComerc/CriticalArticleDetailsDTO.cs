namespace Models.DtosComerc
{
    public class CriticalArticleDetailsDTO
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

        public List<CriticalArticlePerformanceDTO>
            Performanse
        { get; set; } =
                new();

        public decimal MarginEffect { get; set; }

        public decimal VolumeEffect { get; set; }

        public decimal MixEffect { get; set; }

        public decimal TotalDiff { get; set; }
    }
}