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

        public string NazivDobavljaca { get; set; } = string.Empty;

        public decimal Promet { get; set; }

        public decimal RUC12 { get; set; }

        public decimal RUC12Procenat { get; set; }

        public decimal PlanPromet { get; set; }

        public decimal PlanRUC12 { get; set; }

        public decimal PlanRUC12Procenat { get; set; }

        public decimal OdstupanjeRUC12ProcentniPoeni { get; set; }

        public decimal Margina { get; set; }

        public decimal NedostatakMargine { get; set; }

        public DateTime? PoslednjiDatumPodataka { get; set; }
    }
}