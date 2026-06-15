using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class ValeurreferenceEditVM
    {
        public Guid Valeurreferenceid { get; set; }

        [Display(Name = "Analyse")]
        public Guid Idanalyse { get; set; }

        [Display(Name = "Sexe")]
        public string Sexeautorisecode { get; set; } = null!;

        [Display(Name = "Minimum")]
        public int? Agedebut { get; set; }

        [Display(Name = "Maximum")]
        public int? Agefin { get; set; }

        public string Codeunitagedebut { get; set; } = null!;

        public string Codeunitagefin { get; set; } = null!;

        [Display(Name = "De")]
        public decimal? Referencefromvalue { get; set; }

        [Display(Name = "A")]
        public decimal? Referencetovalue { get; set; }

        public string Codeunitreference { get; set; } = null!;

        [Display(Name = "De")]
        public decimal? Referencefromvaluesi { get; set; }

        [Display(Name = "A")]
        public decimal? Referencetovaluesi { get; set; }

        public string Codeunitreferencesi { get; set; } = null!;

        public string? Titre { get; set; }

    }
}
