using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class EnteteDemandePatientVM
    {
        [Required(ErrorMessage = "Le code patient est obligatoire.")]
        [StringLength(50, ErrorMessage = "Le code patient ne peut pas dépasser 50 caractères.")]
        [Display(Name = "Code patient")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string Nom { get; set; } = null!;

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = null!;

        [Display(Name = "Nom d'usage")]
        public string? Nomusage { get; set; }

        [Required(ErrorMessage = "Le sexe est obligatoire.")]
        [Display(Name = "Genre")]
        public string Codesexe { get; set; } = null!;

        [Display(Name = "Date de naissance")]
        public DateTime? Datenaissance { get; set; }

        [Range(0, 130, ErrorMessage = "L'âge doit être compris entre 0 et 130.")]
        [Display(Name = "Âge")]
        public int Age { get; set; }

        [Display(Name = "Type de peau")]
        public string? Codetypepeau { get; set; }

        [Display(Name = "Type de document")]
        public string? Codetypedocumentidentite { get; set; }

        [Display(Name = "Numéro de document")]
        public string? Numerodocumentidentite { get; set; }

        [Required(ErrorMessage = "La ville est obligatoire.")]
        [Display(Name = "Ville")]
        public string Ville { get; set; } = null!;

        [Required(ErrorMessage = "Le quartier est obligatoire.")]
        [Display(Name = "Quartier")]
        public string Quartier { get; set; } = null!;

        [Display(Name = "Renseignements cliniques")]
        public string? Renseignementclinique { get; set; }

        [Display(Name = "Lieu de naissance")]
        public string? Lieunaissance { get; set; }

        [Display(Name = "Téléphone")]
        public string? Tel { get; set; }

        public string? Adresse { get; set; }

        public string? Email { get; set; }

        public IFormFile? File { get; set; }

        public string? Filename { get; set; }

        public string? PhotoBase64 { get; set; }
    }

}

