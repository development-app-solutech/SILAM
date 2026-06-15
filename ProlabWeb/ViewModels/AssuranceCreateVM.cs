using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class AssuranceCreateVM
    {
        [Required(ErrorMessage = "Le code assurance est obligatoire.")]
        [StringLength(16)]
        public string Codeassurance { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(128)]
        public string Nom { get; set; }

        public string? Description { get; set; }

        public bool Isactive { get; set; } = true;

        [Required(ErrorMessage = "Le type d'assurance est obligatoire.")]
        public string Codetypeassurance { get; set; }
    }
} 