namespace GomexPraksaMVC.Models
{
    public class DodajAkcijuViewModel
    {
        public string SifraArtikla { get; set; } = string.Empty;

        public DateOnly DatumOd { get; set; }

        public DateOnly DatumDo { get; set; }

        public decimal AkcijskaCena { get; set; }

        public int TipAkcijeId { get; set; }

        public List<TipAkcijeViewItem> TipoviAkcija { get; set; } = new();
    }
}