using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class ParametreItemCreateVM
    {
        //// Alias pour les vues Create
        //public string Codeunite { get; set; } = null!;
        //public string Codeunitesi { get; set; } = null!;
        //public decimal? Facteurconversion { get; set; }
        //public int? Decimale { get; set; }
        //public int? Decimalesi { get; set; }
        //public int Ordre { get; set; }
        
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Masquer")]
        public bool Masquer { get; set; }

        //[Display(Name = "Formule")]
        //public string? Formule { get; set; }

        [Display(Name = "Résultat Standard")]
        public string? ResultatStandard { get; set; }

        [Display(Name = "Unité Standard")]
        public string UniteStandard { get; set; }

        [Display(Name = "Résultat SI")]
        public string? ResultatSI { get; set; }

        [Display(Name = "Unité SI")]
        public string UniteSI { get; set; }

        [Display(Name = "Facteur Conversion")]
        public string? FacteurConversion { get; set; }

        [Display(Name = "Ordre Affichage")]
        public int OrdreAffichage { get; set; }

        public ParametreItemBuilderVM Builder { get; set; }

        //[Display(Name = "Value builder")]
        //public string? Valuebuilder { get; set; }
    }
} 
