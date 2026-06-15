using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class RecetteParJourVM
    {
        [Required(ErrorMessage = "La date est requise")]
        [Display(Name = "Date")]
        public string Date { get; set; }

        [Required(ErrorMessage = "Le type de client est requis")]
        [Display(Name = "Type de client")]
        public string TypeClient { get; set; }
    }
} 