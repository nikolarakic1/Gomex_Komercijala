namespace Models.DtosComerc
{
    public class CriticalArticlePerformanceDTO
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