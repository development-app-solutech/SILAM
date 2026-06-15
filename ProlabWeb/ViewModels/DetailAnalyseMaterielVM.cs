using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class DetailAnalyseMaterielVM
    {
        [Display(Name = "Matériel")]
        public Guid Materielid { get; set; }

        [Display(Name = "Quantité")]
        [Required(ErrorMessage = "La quantité est obligatoire")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La quantité doit être supérieure à 0")]
        public decimal Quantite { get; set; }
    }
}
