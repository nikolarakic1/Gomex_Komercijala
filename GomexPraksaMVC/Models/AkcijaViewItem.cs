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

        public decimal? RedovnaCena { get; set; }

        public decimal ActualPromet { get; set; }

        public decimal ActualRUC12 { get; set; }

        public decimal ActualRUC12Procenat { get; set; }

        public decimal Kolicina { get; set; }

        public decimal NabavnaVrednost { get; set; }

        public decimal NedostatakMargine { get; set; }

        public decimal MarzaPoKomadu =>
            Kolicina != 0
                ? ActualRUC12 / Kolicina
                : 0;

        public decimal? ProcenatPopusta =>
            RedovnaCena.HasValue &&
            RedovnaCena.Value > 0
                ? Math.Round(
                    (1 - (AkcijskaCena / RedovnaCena.Value))
                    * 100,
                    1
                )
                : null;
    }

    public class AkcijaGrupaViewItem
    {
        public string TipAkcije { get; set; } = string.Empty;

        public DateTime DatumOd { get; set; }

        public DateTime DatumDo { get; set; }

        public int BrojArtikala { get; set; }

        public List<AkcijaViewItem> Artikli { get; set; } = new();

        public decimal UkupanActualPromet =>
            Artikli.Sum(x => x.ActualPromet);

        public decimal UkupanActualRUC12 =>
            Artikli.Sum(x => x.ActualRUC12);

        public decimal UkupnaKolicina =>
            Artikli.Sum(x => x.Kolicina);

        public decimal ActualRUC12Procenat =>
            UkupanActualPromet != 0
                ? UkupanActualRUC12
                  / UkupanActualPromet
                : 0;
    }
}