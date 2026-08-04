namespace Models.Dtos;

public class AkcijaDTO
{
    public int AkcijaId { get; set; }

    public int ArtikalId { get; set; }

    public DateTime DatumOd { get; set; }

    public DateTime DatumDo { get; set; }

    public decimal AkcijskaCena { get; set; }

    public string TipAkcije { get; set; } = string.Empty;
}