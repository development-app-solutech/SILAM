using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurCreateVM
    {        
        [Required(ErrorMessage = "Le matricule est requis.")]
        public string Matricule { get; set; } = null!;

        [Required(ErrorMessage = "Le nom est requis.")]
        public string Nom { get; set; } = null!;

        [Required(ErrorMessage = "Le prénom est requis.")]
        public string Prenom { get; set; } = null!;

        [Required(ErrorMessage = "Le Genre est requis.")]
        [Display(Name = "Genre")]
        public string Codesexe { get; set; } = null!;

        [Required(ErrorMessage = "La date de naissance est requise.")]
        public DateOnly Datenaissance { get; set; }

        [Required(ErrorMessage = "La nationalité est requise.")]
        public string Nationnalite { get; set; }

        [Display(Name = "Téléphone 1")]
        public string? Tel1 { get; set; }

        [Display(Name = "Téléphne 2")]
        public string? Tel2 { get; set; }

        [Required(ErrorMessage = "Le mobile 1 est requis.")]
        [Display(Name = "Mobile 1")]
        public string Mob1 { get; set; } = null!;

        [Display(Name = "Mobile 2")]
        public string? Mob2 { get; set; }

        [Required(ErrorMessage = "L'email est requis.")]
        [RegularExpression(
   @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
   ErrorMessage = "Format email invalide."
)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le site est requis.")]

        [Display(Name = "Site")]
        public string Codesite { get; set; }

        [Display(Name = "ID INH")]
        public string? Idinh { get; set; }

        //public string Userid { get; set; } = null!;
    }
}
