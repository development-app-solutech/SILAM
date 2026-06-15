using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProlabWeb.ViewModels
{
    public class AnalysematerielCreateVM
    {
        [Display(Name = "Analyse")]
        [Required(ErrorMessage = "L'analyse est obligatoire")]
        public Guid Idanalyse { get; set; }

        [Display(Name = "Matériels")]
        public List<DetailAnalyseMaterielVM> Materiels { get; set; } = new List<DetailAnalyseMaterielVM>();
    }
}
