using Models.ModelsDash;

namespace GomexPraksa.Repository;

public interface IArtikalRepo
{
    Task<IEnumerable<Artikal>> GetAllAsync();

    Task<Artikal?> GetByIdAsync(int id);

    Task<Artikal?> GetBySifraAsync(string sifra);

    Task<IEnumerable<Artikal>> SearchAsync(
        string? naziv,
        int? dobavljacId,
        int? robnaGrupaId,
        bool? aktivan);
}