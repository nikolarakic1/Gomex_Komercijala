namespace GomexPraksaMVC.Models
{
    public class AkcijaViewItem
    {
        public int AkcijaId { get; set; }
        public int ArtikalId { get; set; }
        public DateTime DatumOd { get; set; }
        public DateTime DatumDo { get; set; }
        public decimal AkcijskaCena { get; set; }
        public string TipAkcije { get; set; } = string.Empty;

        public string? ArtikalNaziv { get; set; }
        public string? ArtikalSifra { get; set; }
    }

    public class AkcijaGrupaViewItem
    {
        public string TipAkcije { get; set; } = string.Empty;
        public DateTime DatumOd { get; set; }
        public DateTime DatumDo { get; set; }
        public int BrojArtikala { get; set; }
        public List<AkcijaViewItem> Artikli { get; set; } = new();
    }
}