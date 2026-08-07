namespace Models.DtosComerc
{
    public class CriticalProductsDTO
    {
        public int ArtikalId { get; set; }

        public string NazivArtikla { get; set; } = string.Empty;

        public decimal Ruc12 { get; set; }

        public decimal NedostatakMargine { get; set; }
    }
}