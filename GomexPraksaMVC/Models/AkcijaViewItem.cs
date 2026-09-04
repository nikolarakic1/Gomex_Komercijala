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

        // =============================================
        // CM METRIKE
        // =============================================

        public decimal Promet { get; set; }

        public decimal Kolicina { get; set; }

        public decimal Ruc12 { get; set; }

        public decimal Ruc12Procenat { get; set; }

        public decimal NabavnaVrednost { get; set; }

        public decimal NedostatakMargine { get; set; }

        public decimal MarzaPoKomadu =>
            Kolicina != 0
                ? Ruc12 / Kolicina
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

        // =============================================
        // UKUPNO ZA CELU AKCIJU
        // =============================================

        public decimal UkupanPromet =>
            Artikli.Sum(x => x.Promet);

        public decimal UkupanRuc12 =>
            Artikli.Sum(x => x.Ruc12);

        public decimal UkupnaKolicina =>
            Artikli.Sum(x => x.Kolicina);

        public decimal Ruc12Procenat =>
            UkupanPromet != 0
                ? UkupanRuc12 / UkupanPromet
                : 0;
    }
}