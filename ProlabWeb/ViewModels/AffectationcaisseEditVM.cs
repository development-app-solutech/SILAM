using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class AffectationcaisseEditVM
    {
        [Required]
        public Guid Affectationcaisseid { get; set; }

        [Display(Name = "Caissie")]
        [Required]
        public Guid Caisseid { get; set; }

        [Display(Name = "Caissier")]
        [Required]
        public Guid Utilisateurid { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
