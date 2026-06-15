using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ProlabWeb.ViewModels
{
    public class ParametreResultatVM
    {
        public Guid Parametreid { get; set; }
        
        public string Nom { get; set; } = string.Empty;
        
        public string Code { get; set; } = string.Empty;
        
        public string Unite { get; set; } = string.Empty;
        
        public string? Resultat { get; set; }
        
        public string UniteSI { get; set; } = string.Empty;
        
        public string? Resultatsi { get; set; }

        public string? Commentaire { get; set; }

        [ValidateNever]
        public ParametreItemBuilderVM Builder { get; set; } = new ParametreItemBuilderVM();

        public string? FacteurConversion { get; set; }
    }
} 