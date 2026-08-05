using System;
using System.Collections.Generic;
using System.Text;

namespace Models.DtosComerc
{
    public class RucChangeDTO
    {
        public decimal PocetniRuc { get; set; }

        public decimal MarginEffect { get; set; }

        public decimal VolumeEffect { get; set; }

        public decimal MixEffect { get; set; }

        public decimal UkupnaPromena { get; set; }

        public decimal UkupnaPromenaProcenat { get; set; }

        public decimal KonacniRuc { get; set; }
    }
}
