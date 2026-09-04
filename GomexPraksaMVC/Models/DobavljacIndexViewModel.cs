namespace GomexPraksaMVC.Models
{
    public class DobavljacIndexViewModel
    {
        public List<DobavljacViewItem> Dobavljaci { get; set; } = new();

        public string? Naziv { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }
    }
}