using GomexPraksa.AddedFunctions;
using Models.ModelsDash;

namespace GomexPraksa.Repository;

public interface IArtikalRepo
{
    public Task<PaginationGeneric<Artikal>> GetAllAsync(
    bool canViewAllCategories,
    List<int> kategorijaIds,
    PaginationParams paginationArtikli);

    public Task<Artikal?> GetByIdAsync(int id, bool canViewAllCategories,
    List<int> kategorijaIds);

    Task<Artikal?> GetBySifraAsync(string sifra,bool canViewAllCategories,List<int> kategorijaIds);

    Task<PaginationGeneric<Artikal>> SearchAsync(
        string? naziv,
        int? dobavljacId,
        int? robnaGrupaId,
        bool? aktivan,
        bool CanViewAllCategories,
        List<int> kategorijaIds,
        PaginationParams paginationArtikli);
}