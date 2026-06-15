using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class EnteteresultatCreateVM
    {
        [Required(ErrorMessage = "Le site est requis")]
        [Display(Name = "Site")]
        public string Codesite { get; set; } = null!;

        [Required(ErrorMessage = "Le technicien est requis")]
        [Display(Name = "Technicien")]
        public Guid Technicienid { get; set; }

        [Required(ErrorMessage = "Le laboratoire est requis")]
        [Display(Name = "Laboratoire")]
        public int Laboratoireid { get; set; }

        [Required(ErrorMessage = "La demande est requise")]
        [Display(Name = "Demande d'analyse")]
        public Guid Entetedemandeid { get; set; }

        [Required(ErrorMessage = "La date est requise")]
        public DateTime Date { get; set; }

        public string? Interpretation { get; set; }

        [Required(ErrorMessage = "L'analyse est requise")]
        [Display(Name = "Analyse")]
        public Guid Idanalyse { get; set; }

        // Heures de prélèvement et réception
        [Display(Name = "Heure de prélèvement")]
        public DateTime? DatePrelevement { get; set; }

        [Display(Name = "Heure d'arrivée au labo")]
        public DateTime? DateReception { get; set; }

        // Liste des paramètres pour le tableau
        public List<ParametreResultatVM> Parametres { get; set; } = new List<ParametreResultatVM>();
    }
} 