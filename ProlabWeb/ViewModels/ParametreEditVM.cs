using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class ParametreEditVM
    {
        [Display(Name = "Laboratoire")]
        [Required]
        public int IdLaboratoire { get; set; }

        [Display(Name = "Nom du Laboratoire")]
        public string? NomLaboratoire { get; set; }

        [Display(Name = "Analyse")]
        [Required]
        public Guid IdAnalyse { get; set; }

        public List<ParametreItemEditVM> Parametres { get; set; } = new List<ParametreItemEditVM>();
    }
}