using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurEditVM
    {
        public Guid Utilisateurid { get; set; }
        public string Matricule { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;

        [Display(Name = "Genre")]
        public string Codesexe { get; set; } = null!;

        public DateOnly Datenaissance { get; set; }
        public string Nationnalite { get; set; } = null!;

        [Display(Name = "Téléphone 1")]
        public string? Tel1 { get; set; }

        [Display(Name = "Téléphone 2")]
        public string? Tel2 { get; set; }

        [Required(ErrorMessage = "Le mobile 1 est requis.")]
        [Display(Name = "Mobile 1")]
        public string Mob1 { get; set; } = null!;

        [Display(Name = "Mobile 2")]
        public string? Mob2 { get; set; }

        [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Format email invalide."
    )]
        public string Email { get; set; } = null!;

        [Display(Name = "Site")]
        public string Codesite { get; set; }
        
        [Display(Name = "ID INH")]
        public string? Idinh { get; set; }

        public IEnumerable<SelectListItem>? Pays { get; set; }
        public IEnumerable<SelectListItem>? Sites { get; set; }

        // ASP.NET Identity roles
        public List<UtilisateurRoleVM> AvailableRoles { get; set; } = new List<UtilisateurRoleVM>();

        public string? UserId { get; set; }

        // Pour la soumission du formulaire
        public List<string> SelectedRoleIds { get; set; } = new List<string>();
        
        // Signature pour les biologistes
        [Display(Name = "Signature")]
        public IFormFile? SignatureFile { get; set; }
        
        [Display(Name = "Signature actuelle")]
        public string? CurrentSignatureBase64 { get; set; }
        
        public bool HasCurrentSignature => !string.IsNullOrEmpty(CurrentSignatureBase64);
        
        // Pour la signature canvas (base64)
        public string? CanvasSignatureData { get; set; }
    }

}
