using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class RecetteParPeriodeVM
    {
        [Required(ErrorMessage = "La date de début est requise")]
        [Display(Name = "Date de début")]
        public string Debut { get; set; } = null!;

        [Required(ErrorMessage = "La date de fin est requise")]
        [Display(Name = "Date de fin")]
        public string Fin { get; set; } = null!;

        [Display(Name = "Type de client")]
        public string? TypeClient { get; set; }
    }
}
