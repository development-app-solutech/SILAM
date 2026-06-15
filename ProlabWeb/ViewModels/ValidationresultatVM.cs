using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProlabWeb.ViewModels
{
    public class ValidationresultatVM
    {
        public Guid Enteteresultatid { get; set; }

        [Display(Name = "Site")]
        public string Codesite { get; set; }
        public string? SiteNom { get; set; }

        [Display(Name = "Technicien")]
        public Guid Technicienid { get; set; }

        [Display(Name = "Biologiste")]
        [Required(ErrorMessage = "Le biologiste est requis")]
        public Guid Biologisteid { get; set; }

        [Display(Name = "Demande d'analyse")]
        public Guid Entetedemandeid { get; set; }

        public DateTime Date { get; set; }

        public string? Interpretation { get; set; }

        [Display(Name = "Analyse")]
        public Guid Idanalyse { get; set; }

        public List<ParametreResultatVM> Parametres { get; set; } = new List<ParametreResultatVM>();

        public string? TechnicienNom { get; set; }
        public string? TechnicienPrenom { get; set; }
        public string? DemandeNumero { get; set; }
        public DateTime? DemandeDate { get; set; }
        public string? PatientNom { get; set; }
        public string? PatientPrenom { get; set; }
        public string? PatientCode { get; set; }
        public string? PrescripteurNom { get; set; }
        public string? AssuranceNom { get; set; }
        public decimal? AssuranceTaux { get; set; }
        public string? AnalyseNom { get; set; }
        public string? Statut { get; set; }

        // Pour la signature canvas (si pas de signature automatique)
        public string? CanvasSignatureData { get; set; }

        // Pour afficher la signature du biologiste (depuis Signatureutilisateurs)
        public string? BiologisteSignatureBase64 { get; set; }
        public bool HasBiologisteSignature { get; set; }
        
        // Nom du biologiste récupéré via la relation
        public string? BiologisteNomComplet { get; set; }
        
        // Informations sur les échantillons et prélèvements
        [Display(Name = "Date et heure d'arrivée")]
        public DateTime? HeureArrivee { get; set; }
        
        [Display(Name = "Nature des échantillons")]
        public string? NatureEchantillons { get; set; }
        
        [Display(Name = "Date et heure de prélèvement")]
        public DateTime? DatePrelevement { get; set; }
        
        [Display(Name = "Lieu de prélèvement")]
        public string? LieuPrelevement { get; set; }
        
    }
}
