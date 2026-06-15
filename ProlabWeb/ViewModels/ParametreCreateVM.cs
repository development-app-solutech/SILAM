using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class ParametreCreateVM
    {
        [Display(Name = "Laboratoire")]
        [Required]
        public int IdLaboratoire { get; set; }

        [Display(Name = "Nom du Laboratoire")]
        public string? NomLaboratoire { get; set; }

        [Display(Name = "Analyse")]
        [Required]
        public Guid IdAnalyse { get; set; }

        public List<ParametreItemCreateVM> Parametres { get; set; } = new List<ParametreItemCreateVM>();
    }
}