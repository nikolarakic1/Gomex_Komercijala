namespace GomexPraksa.AddedFunctions
{
    public class PaginationGeneric<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => 
            PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage =>
        Page > 1;

        public bool HasNextPage =>
            Page < TotalPages;
    }
}
