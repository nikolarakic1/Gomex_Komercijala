using Models.DtosComerc;

namespace GomexPraksa.AddedFunctions
{
    public interface ICriticalArticleDetailsService
    {
        Task<CriticalArticleDetailsDTO?>
            GetDetailsAsync(
                string sifra,
                int godinaOd,
                int godinaDo);
    }
}