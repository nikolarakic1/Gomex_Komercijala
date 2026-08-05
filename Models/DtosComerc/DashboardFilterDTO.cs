namespace Models.DtosComerc;

public class DashboardFilterDTO
{
    public int Godina { get; set; }

    public int NedeljaOd { get; set; }

    public int NedeljaDo { get; set; }

    public int? OdeljenjeId { get; set; }

    public int? KategorijaId { get; set; }

    public int? DobavljacId { get; set; }

    public int? TipProdajeId { get; set; }
}