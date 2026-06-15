using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class PrelevementEditVM
    {
        [Required]
        public Guid Prelevementid { get; set; }

        [Required(ErrorMessage = "Le détail de la demande est obligatoire.")]
        [Display(Name = "Détail demande")]
        public Guid Detaildemandeid { get; set; }

        [Required(ErrorMessage = "Le préleveur est obligatoire.")]
        [Display(Name = "Préleveur")]
        public Guid Preleveurid { get; set; }

        [Required(ErrorMessage = "La nature de l'échantillon est obligatoire.")]
        [Display(Name = "Nature échantillon")]
        public int Idnatureechantillon { get; set; }

        [Required(ErrorMessage = "La date de prélèvement est obligatoire.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date de prélèvement")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Le statut est obligatoire.")]
        [Display(Name = "Statut")]
        public string Statut { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Date de réception")]
        public DateTime? Datereception { get; set; }
    }
}
