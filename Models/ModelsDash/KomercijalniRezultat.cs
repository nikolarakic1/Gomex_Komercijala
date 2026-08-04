namespace Models.ModelsDash;
    public class KomercijalniRezultat
    {
        public long KomercijalniRezultatId { get; set; }

        public int ArtikalId { get; set; }

        public int TipProdajeId { get; set; }

        public short Godina { get; set; }

        public byte Nedelja { get; set; }

        public bool CMUtice { get; set; }

        public bool PlusIzvoz { get; set; }

        public decimal Kolicina { get; set; }

        public decimal MPBezPDV { get; set; }

        public decimal NabavnaVrednostBezPDV { get; set; }

        public decimal RUC12 { get; set; }

        public decimal RUC12Procenat { get; set; }

        public decimal NedostatakMargine { get; set; }

        public decimal MixPlusMargina { get; set; }

        public decimal MarginEffect { get; set; }

        public decimal VolumeEffect { get; set; }

        public decimal MixEffect { get; set; }

        public decimal TotalDiff { get; set; }

        public DateTime DatumUnosa { get; set; }

        public Artikal? Artikal { get; set; }

        public TipProdaje? TipProdaje { get; set; }
    }
