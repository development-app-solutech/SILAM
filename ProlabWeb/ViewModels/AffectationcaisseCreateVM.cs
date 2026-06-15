using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels

{
    public class AffectationcaisseCreateVM
    {
        [Display(Name = "Caisse")]
        [Required]
        public Guid Caisseid { get; set; }

        [Display(Name = "Caissier")]
        [Required]
        public Guid Utilisateurid { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
