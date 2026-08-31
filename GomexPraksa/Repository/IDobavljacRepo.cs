using Models.ModelsDash;

namespace GomexPraksa.Repository
{
    public interface IDobavljacRepo
    {
        public Task<IEnumerable<Dobavljac>> GetAllDobavljace(bool CanViewAllCategories,List<int> KategorijaIds);
        public Task<Dobavljac?> GetByIdAsync(int id, bool CanViewAllCategories , List<int>KategorijaIds);
        Task<IEnumerable<Dobavljac>> SearchAsync(
       string? naziv,
       bool? aktivan,bool CanViewAllCategories,List<int>KategorijaIds);
        Task<IEnumerable<Dobavljac>> CriticalDobavljaciAsync(bool canViewAllCategories,List<int>KategorijaIds);
        
    }
}
