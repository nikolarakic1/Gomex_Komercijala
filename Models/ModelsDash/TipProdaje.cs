namespace Models.ModelsDash;
public class TipProdaje
{
    public int TipProdajeId { get; set; }

    public string Naziv { get; set; } = string.Empty;

    public int? NadredjeniTipId { get; set; }

    public bool Aktivan { get; set; }

    public TipProdaje? NadredjeniTip { get; set; }


}