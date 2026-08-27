using Models.ModelsDash;

namespace GomexPraksa.Repository;

public interface IArtikalRepo
{
    Task<IEnumerable<Artikal>> GetAllAsync(
    bool canViewAllCategories,
    List<int> kategorijaIds);

    public Task<Artikal?> GetByIdAsync(int id, bool canViewAllCategories,
    List<int> kategorijaIds);

    Task<Artikal?> GetBySifraAsync(string sifra,bool canViewAllCategories,List<int> kategorijaIds);

    Task<IEnumerable<Artikal>> SearchAsync(
        string? naziv,
        int? dobavljacId,
        int? robnaGrupaId,
        bool? aktivan,
        bool CanViewAllCategories,
        List<int> kategorijaIds);
}