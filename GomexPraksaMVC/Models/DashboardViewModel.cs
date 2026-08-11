namespace GomexPraksaMVC.Models
{
    public class DashboardViewModel
    {
        public decimal PrometBezPdv { get; set; }
        public decimal PrometPromenaProcenat { get; set; }

        public decimal Ruc12 { get; set; }
        public decimal Ruc12PromenaProcenat { get; set; }

        public decimal Ruc12Procenat { get; set; }
        public decimal Ruc12PromenaProcentniPoeni { get; set; }

        public int KriticniArtikli { get; set; }
        public int KriticniArtikliPromena { get; set; }

        public decimal NedostatakMarze { get; set; }
        public decimal NedostatakMarzePromenaProcenat { get; set; }

        public DateTime? PodaciOsvezeni { get; set; }

        public List<DobavljacViewItem> Dobavljaci { get; set; } = new();
        public int? SelectedDobavljacId { get; set; }
        // Filter state - added to support passing filters from MVC to API
        public DateOnly? DatumOd { get; set; }
        public DateOnly? DatumDo { get; set; }
        public int? OdeljenjeId { get; set; }
        public int? KategorijaId { get; set; }
        public int? TipProdajeId { get; set; }

        // Local lookup lists (populated in controller). These are temporary until backend exposes lookup endpoints.
        public List<OdeljenjeViewItem> Odeljenja { get; set; } = new();
        public List<KategorijaViewItem> Kategorije { get; set; } = new();
        public List<TipProdajeViewItem> TipoviProdaje { get; set; } = new();
        // Critical products for Top5 chart (view models)
        public List<CriticalProductViewItem> CriticalTop5 { get; set; } = new();
        // Optional paged critical products
        public List<CriticalProductPageViewItem> CriticalPage { get; set; } = new();
        // RUC change breakdown (waterfall)
        public RucChangeViewItem? RucChange { get; set; }
    }

    public class DobavljacViewItem
    {
        public int DobavljacId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool Aktivan { get; set; }
    }

    public class RucChangeViewItem
    {
        public decimal PocetniRuc { get; set; }
        public decimal MarginEffect { get; set; }
        public decimal VolumeEffect { get; set; }
        public decimal MixEffect { get; set; }
        public decimal UkupnaPromena { get; set; }
        public decimal UkupnaPromenaProcenat { get; set; }
        public decimal KonacniRuc { get; set; }
    }

    public class OdeljenjeViewItem
    {
        public int OdeljenjeId { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }

    public class KategorijaViewItem
    {
        public int KategorijaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int OdeljenjeId { get; set; }
    }

    public class TipProdajeViewItem
    {
        public int TipProdajeId { get; set; }
        public string Naziv { get; set; } = string.Empty;
    }

    public class CriticalProductViewItem
    {
        public int ArtikalId { get; set; }
        public string NazivArtikla { get; set; } = string.Empty;
        public string Kategorija { get; set; } = string.Empty;
        public string Severnost { get; set; } = string.Empty;
        public decimal ProcenjeniUticaj { get; set; }
    }

    public class CriticalProductPageViewItem
    {
        public int ArtikalId { get; set; }
        public string Sifra { get; set; } = string.Empty;
        public string Naziv { get; set; } = string.Empty;
        public string? Dobavljac { get; set; }

        public decimal Promet { get; set; }
        public decimal RUC12 { get; set; }
        public decimal RUC12Procenat { get; set; }
        public decimal NedostatakMargine { get; set; }
    }
}