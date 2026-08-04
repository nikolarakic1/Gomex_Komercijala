namespace Models.ModelsDash
{
    public class Kategorija
    {
        public int KategorijaId { get; set; }

        public int OdeljenjeId { get; set; }

        public Odeljenje? Odeljenje { get; set; }

        public string Naziv { get; set; } = string.Empty;

        
    }
}