using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class EnteteDemandeEditVM
    {
        public Guid Entetedemandeid { get; set; }

        public string Numero { get; set; } = null!;

        [Display(Name = "Site")]
        public string Codesite { get; set; } = null!;

        [Display(Name = "Créé par")]
        public Guid Utilisateurid { get; set; }

        [Display(Name = "Partenaire")]
        public Guid? Partenaireid { get; set; }

        [Display(Name = "Rechercher un patient")]
        public Guid Patientid { get; set; }

        [ValidateNever]
        public EnteteDemandePatientVM Patient { get; set; } = new EnteteDemandePatientVM();

        [Display(Name = "Catégories")]
        public Guid[]? IdsCategorie { get; set; } = new Guid[0];

        [Display(Name = "Analyses")]
        public Guid[]? IdsAnalyse { get; set; } = new Guid[0];

        [Display(Name = "Est assuré ?")]
        public bool EstAssure { get; set; }

        [Display(Name = "Assurance")]
        public string? Codeassurance { get; set; } = null!;

        [Display(Name = "Police d'assurance")]
        public Guid? Policeassuranceid { get; set; }

        [Display(Name = "Rechercher un prescripteur")]
        public Guid Prescripteurid { get; set; }

        [ValidateNever]
        public EntetedemandePrescripteurVM Prescripteur { get; set; } = new EntetedemandePrescripteurVM();
    }
}
