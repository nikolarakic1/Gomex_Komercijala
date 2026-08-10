using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Dtos
{
    public class FilterSharedPages
    {
            public DateTime DatumOd { get; set; }
            public DateTime DatumDo { get; set; }

            public int? OdeljenjeId { get; set; }
            public int? KategorijaId { get; set; }
            public int? DobavljacId { get; set; }
            public int? TipProdajeId { get; set; }   
    }
}
