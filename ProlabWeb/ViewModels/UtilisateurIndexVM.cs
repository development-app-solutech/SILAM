using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurIndexVM
    {
        public Guid Utilisateurid { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;

        [Display(Name = "Genre")]
        public string Codesexe { get; set; } = string.Empty;

        [Display(Name = "Date de naissance")]
        public DateOnly Datenaissance { get; set; }
        public string Nationnalite { get; set; } = string.Empty;

        [Display(Name = "Site")]
        public string Codesite { get; set; } = string.Empty;
    }
}
