namespace Models.DtosComerc;

public class DashboardFilterDTO
{
    public DateOnly? DatumOd { get; set; }

    public DateOnly? DatumDo { get; set; }

    public int? OdeljenjeId { get; set; }

    public int? KategorijaId { get; set; }

    public int? DobavljacId { get; set; }

    public int? TipProdajeId { get; set; }
}