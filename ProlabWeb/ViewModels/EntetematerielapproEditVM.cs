using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProlabWeb.ViewModels
{
    public class EntetematerielapproEditVM
    {
        [Required]
        public Guid Entetematerielapproid { get; set; }

        [Display(Name = "Date d'approvisionnement")]
        [Required]
        public DateTime Date { get; set; }

        [Display(Name = "Numéro")]
        [Required]
        public string Numero { get; set; }

        [Display(Name = "Utilisateur")]
        [Required]
        public Guid Utilisateurid { get; set; }

        [Display(Name = "Fournisseur")]
        [Required]
        public Guid Fournisseurid { get; set; }

        public List<DetailMaterielApproVM> Materiels { get; set; } = new List<DetailMaterielApproVM>();
    }
} 