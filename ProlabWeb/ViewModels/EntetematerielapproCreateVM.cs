using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProlabWeb.ViewModels
{
    public class EntetematerielapproCreateVM
    {
        [Display(Name = "Date d'approvisionnement")]
        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "Numéro")]
        [Required]
        public string Numero { get; set; } = string.Empty;

        [Display(Name = "Utilisateur")]
        [Required]
        public Guid Utilisateurid { get; set; }

        [Display(Name = "Fournisseur")]
        [Required]
        public Guid Fournisseurid { get; set; }

        public List<DetailMaterielApproVM> Materiels { get; set; } = new List<DetailMaterielApproVM>();
    }
} 