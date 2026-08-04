using Models.ReadDetails;

namespace GomexPraksa.Repository
{
    public interface IAkcijaRepo
    {
        Task<IEnumerable<AkcijaDetalji>> GetAllAsync();

        Task<AkcijaDetalji?> GetByIdAsync(int id);

        Task<IEnumerable<AkcijaDetalji>> GetTrenutneAsync();

        Task<IEnumerable<AkcijaDetalji>> GetBuduceAsync();

        Task<IEnumerable<AkcijaDetalji>> GetByArtikalIdAsync(int artikalId);

        Task<AkcijaDetalji?> GetPoslednjuZaArtikalAsync(int artikalId);
    }
}