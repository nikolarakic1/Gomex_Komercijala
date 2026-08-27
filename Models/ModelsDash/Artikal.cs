namespace Models.ModelsDash
{
    public class Artikal
    {
        public int ArtikalId { get; set; }

        public string Sifra { get; set; } = string.Empty;

        public string Naziv { get; set; } = string.Empty;

        public int DobavljacId { get; set; }

        public Dobavljac? Dobavljac { get; set; }

        public int RobnaGrupaId { get; set; }

        public RobnaGrupa? RobnaGrupa { get; set; }

        public bool Aktivan { get; set; }
        public decimal? RedovnaCena { get; set; }


<<<<<<< HEAD
=======
        public decimal RedovnaCena { get; set; }

>>>>>>> 79e71937660ae51f2d6ef2c2747c1f2d2b96389f
    }
}