using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class PoliceassuranceEditVM
    {
        [Required]
        public Guid Policeassuranceid { get; set; }

        [Required(ErrorMessage = "L'assurance est obligatoire.")]
        [Display(Name = "Assurance")]
        public string Codeassurance { get; set; }

        [Required(ErrorMessage = "Le libellé est obligatoire.")]
        [Display(Name = "Libellé")]
        public string Libelle { get; set; }

        [Required(ErrorMessage = "Le taux est obligatoire.")]
        [Range(0, 100, ErrorMessage = "Le taux doit être compris entre 0 et 100.")]
        [Display(Name = "Taux (%)")]
        public decimal Taux { get; set; }
    }
}
