namespace Models.ModelsDash
{
    public class RobnaGrupa
    {
        public int RobnaGrupaId { get; set; }

        public int KategorijaId { get; set; }

        public Kategorija? Kategorija { get; set; }

        public string Naziv { get; set; } = string.Empty;

        
    }
}