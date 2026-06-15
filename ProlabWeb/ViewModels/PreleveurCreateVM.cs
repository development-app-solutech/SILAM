using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class PreleveurCreateVM
    {
        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le sexe est obligatoire.")]
        [Display(Name = "Sexe")]
        public string Codesexe { get; set; }

        [Required(ErrorMessage = "La date de naissance est obligatoire.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateOnly Datenaissance { get; set; }

        [Display(Name = "Téléphone 1")]
        public string? Tel1 { get; set; }

        [Display(Name = "Téléphone 2")]
        public string? Tel2 { get; set; }

        [Required(ErrorMessage = "Le mobile 1 est obligatoire.")]
        [Display(Name = "Mobile 1")]
        public string Mob1 { get; set; }

        [Display(Name = "Mobile 2")]
        public string? Mob2 { get; set; }
        [EmailAddress(ErrorMessage = "Adresse email invalide.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Le type est obligatoire.")]
        [Display(Name = "Type")]
        public string Type { get; set; }
    }
}
