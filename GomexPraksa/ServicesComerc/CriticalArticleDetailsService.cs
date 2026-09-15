using GomexPraksa.AddedFunctions;
using Models.DtosComerc;

namespace GomexPraksa.ServiceComerc
{
    public class CriticalArticleDetailsService
        : ICriticalArticleDetailsService
    {
        private readonly ICriticalArticleDetailsRepo _repo;

        public CriticalArticleDetailsService(
            ICriticalArticleDetailsRepo repo)
        {
            _repo = repo;
        }

        public async Task<CriticalArticleDetailsDTO?>
            GetDetailsAsync(
                string sifra,
                int godinaOd,
                int godinaDo)
        {
            if (string.IsNullOrWhiteSpace(sifra))
            {
                throw new ArgumentException(
                    "Šifra artikla je obavezna."
                );
            }

            if (godinaOd < 2000)
            {
                throw new ArgumentException(
                    "Početna godina nije validna."
                );
            }

            if (godinaDo < 2000)
            {
                throw new ArgumentException(
                    "Završna godina nije validna."
                );
            }

            if (godinaOd > godinaDo)
            {
                throw new ArgumentException(
                    "Početna godina ne može biti veća od završne."
                );
            }

            if (godinaDo > DateTime.Today.Year)
            {
                throw new ArgumentException(
                    "Završna godina ne može biti u budućnosti."
                );
            }

            var rezultat =
                await _repo.GetDetailsAsync(
                    sifra.Trim(),
                    godinaOd,
                    godinaDo
                );

            if (rezultat == null)
            {
                return null;
            }

            rezultat.Performanse =
                rezultat.Performanse
                    .OrderBy(x => x.Godina)
                    .ThenBy(x =>
                        x.Tip.Equals(
                            "ACTUAL",
                            StringComparison.OrdinalIgnoreCase
                        )
                            ? 0
                            : 1
                    )
                    .ThenBy(x =>
                        x.VrstaProdaje.Equals(
                            "AKCIJSKA",
                            StringComparison.OrdinalIgnoreCase
                        )
                            ? 0
                            : 1
                    )
                    .ThenBy(x => x.TipAkcije)
                    .ToList();

            return rezultat;
        }
    }
}