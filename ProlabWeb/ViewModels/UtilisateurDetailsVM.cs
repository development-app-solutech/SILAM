using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurDetailsVM
    {
        public Guid Utilisateurid { get; set; }
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;

        [Display(Name = "Genre")]
        public string Codesexe { get; set; } = null!;
        public DateOnly Datenaissance { get; set; }
        public string Nationnalite { get; set; } = null!;

        [Display(Name = "Site")]
        public string Codesite { get; set; } = null!;
        public string Matricule { get; set; } = null!;
        public string? Tel1 { get; set; }
        public string? Tel2 { get; set; }
        public string Mob1 { get; set; } = null!;
        public string? Mob2 { get; set; }
        public string Email { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string? PasswordToShow { get; set; } // Mot de passe temporaire à afficher
        public bool Isactive { get; set; }
        public bool Mustchangepass { get; set; }
    }
}
