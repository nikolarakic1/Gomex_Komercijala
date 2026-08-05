using Models.ModelsDash;

namespace GomexPraksa.Repository
{
    public interface IDobavljacRepo
    {
        public Task<IEnumerable<Dobavljac>> GetAllDobavljace();
        public Task<Dobavljac?> GetByIdAsync(int id);
        Task<IEnumerable<Dobavljac>> SearchAsync(
       string? naziv,
       bool? aktivan);
    }
}
