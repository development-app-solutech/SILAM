using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurManyLaboratoireEditVM
    {
        [Required(ErrorMessage = "Veuillez sélectionner un utilisateur.")]
        [Display(Name = "Utilisateur")]
        public Guid Utilisateurid { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner le(s) laboratoire(s).")]
        [Display(Name = "Laboratoire")]
        public int[] Idslaboratoire { get; set; }
    }
}
