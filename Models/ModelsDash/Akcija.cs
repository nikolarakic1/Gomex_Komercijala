namespace Models.ModelsDash;

public class Akcija
{
    public int AkcijaId { get; set; }

    public int ArtikalId { get; set; }

    public DateTime DatumOd { get; set; }

    public DateTime DatumDo { get; set; }

    public decimal AkcijskaCena { get; set; }

    public int TipAkcijeId { get; set; }
}