using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class TitreRecetteClientPayantVM
    {
        [Required(ErrorMessage = "La date de début est obligatoire.")]
        [Display(Name = "Date de début")]
        public DateTime Debut { get; set; }

        [Required(ErrorMessage = "La date de fin est obligatoire.")]
        [Display(Name = "Date de fin")]
        public DateTime Fin { get; set; }
    }
}
