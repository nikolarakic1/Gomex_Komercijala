namespace GomexPraksaMVC.Models
{
    public class TipAkcijeViewItem
    {
        public int TipAkcijeId { get; set; }

        public string Naziv { get; set; } = string.Empty;
        public bool Aktivan { get; set; }
    }
}