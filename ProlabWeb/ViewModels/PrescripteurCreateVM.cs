using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class PrescripteurCreateVM
    {
        public string Nom { get; set; } = null!;

        public string? Description { get; set; }

        [Display(Name = "Activé ?")]
        public bool Isactive { get; set; }

        public string? Adresse { get; set; }

        [Display(Name = "Téléphone")]
        public string? Tel { get; set; }
    }
}
