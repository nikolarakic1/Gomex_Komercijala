using Models.DtosComerc;

namespace GomexPraksa.AddedFunctions
{
    public interface ICriticalArticleDetailsRepo
    {
        Task<CriticalArticleDetailsDTO?>
            GetDetailsAsync(
                string sifra,
                int godinaOd,
                int godinaDo);
    }
}