using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class ParametreItemBuilderVM
    {
        [Display(Name = "Type")]
        public string Type { get; set; }

        [Display(Name = "Valeurs / Formule")]
        public string? Valeur { get; set; }
    }
}
