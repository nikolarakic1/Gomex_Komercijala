namespace GomexPraksaMVC.Models
{
    public class DobavljacDetaljiViewModel
    {
        public DobavljacViewItem Dobavljac { get; set; } = new();

        public List<ArtikalViewItem> Artikli { get; set; } = new();

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }
    }
}