namespace Models.DtosComerc
{
    public class CriticalArticleComparisonDTO
    {
        public int Godina1 { get; set; }

        public int Godina2 { get; set; }

        public decimal PrometGodina1 { get; set; }

        public decimal PrometGodina2 { get; set; }

        public decimal RucGodina1 { get; set; }

        public decimal RucGodina2 { get; set; }

        public decimal RucProcenatGodina1 { get; set; }

        public decimal RucProcenatGodina2 { get; set; }

        public decimal PrometRazlika { get; set; }

        public decimal RucRazlika { get; set; }

        public decimal RucRazlikaProcentniPoeni { get; set; }
    }
}