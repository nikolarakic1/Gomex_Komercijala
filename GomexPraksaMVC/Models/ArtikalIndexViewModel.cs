using System.Collections.Generic;

namespace GomexPraksaMVC.Models
{
    public class ArtikalIndexViewModel
    {
        public List<ArtikalViewItem> Artikli { get; set; } = new();
        public List<DobavljacViewItem> Dobavljaci { get; set; } = new();
        public List<OdeljenjeViewItem> Odeljenja { get; set; } = new();
        public List<KategorijaViewItem> Kategorije { get; set; } = new();

        public int? SelectedDobavljacId { get; set; }
        public int? SelectedRobnaGrupaId { get; set; }
        public int? SelectedOdeljenjeId { get; set; }
        public int? SelectedKategorijaId { get; set; }
        public string? Naziv { get; set; }
    }
}
