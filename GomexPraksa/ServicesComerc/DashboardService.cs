using GomexPraksa.RepositoryComerc;
using Models.DtosComerc;

namespace GomexPraksa.ServicesComerc;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepo _repo;

    public DashboardService(IDashboardRepo repo)
    {
        _repo = repo;
    }

    public async Task<DashboardSummaryDTO> FillCardsAsync(
        DashboardFilterDTO filterDTO)
    {
        if (filterDTO is null)
        {
            throw new ArgumentNullException(nameof(filterDTO));
        }

        bool imaDatumOd = filterDTO.DatumOd.HasValue;
        bool imaDatumDo = filterDTO.DatumDo.HasValue;

        if (imaDatumOd != imaDatumDo)
        {
            throw new ArgumentException(
                "Moraju biti uneti i DatumOd i DatumDo, ili nijedan.");
        }

        if (imaDatumOd &&
            filterDTO.DatumOd!.Value > filterDTO.DatumDo!.Value)
        {
            throw new ArgumentException(
                "DatumOd ne može biti posle DatumDo.");
        }

        if (filterDTO.OdeljenjeId.HasValue &&
            filterDTO.OdeljenjeId.Value <= 0)
        {
            throw new ArgumentException(
                "OdeljenjeId mora biti veći od nule.");
        }

        if (filterDTO.KategorijaId.HasValue &&
            filterDTO.KategorijaId.Value <= 0)
        {
            throw new ArgumentException(
                "KategorijaId mora biti veći od nule.");
        }

        if (filterDTO.DobavljacId.HasValue &&
            filterDTO.DobavljacId.Value <= 0)
        {
            throw new ArgumentException(
                "DobavljacId mora biti veći od nule.");
        }

        if (filterDTO.TipProdajeId.HasValue &&
            filterDTO.TipProdajeId.Value <= 0)
        {
            throw new ArgumentException(
                "TipProdajeId mora biti veći od nule.");
        }

        return await _repo.FillCardsAsync(filterDTO);
    }
}