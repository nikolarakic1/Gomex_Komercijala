namespace Models.DtosComerc
{
    public class CriticalProductsDTO
    {
        public int ArtikalId { get; set; }

        public string NazivArtikla { get; set; } = string.Empty;

        public string Kategorija { get; set; } = string.Empty;
        public string Severnost { get; set; } = string.Empty;
        public decimal ProcenjeniUticaj { get; set; }

    }
}