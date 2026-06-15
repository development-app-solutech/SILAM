using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class UtilisateurlaboratoireCreateVM
    {
        public Guid Utilisateurlaboratoireid { get; set; }
        [Required(ErrorMessage = "Veuillez sélectionner un utilisateur.")]
        public Guid Utilisateurid { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un laboratoire.")]
        public int Idlaboratoire { get; set; }
    }
} 