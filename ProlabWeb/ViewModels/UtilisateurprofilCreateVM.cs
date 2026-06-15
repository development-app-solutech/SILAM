using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurprofilCreateVM
    {
        public Guid Utilisateurprofilid { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un utilisateur.")]
        public Guid Utilisateurid { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un profil.")]
        public string Profilid { get; set; }
    }
} 