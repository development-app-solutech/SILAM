using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class AnalyseEditVM
    {
        public Guid Idanalyse { get; set; }

        //[Remote(action: "IsNomAvailable", controller: "Analyse")]
        public string Nom { get; set; }

        [Display(Name = "Code")]
        public string? Codeparametre { get; set; }

        [Display(Name = "Alias dans l'automate")]
        public string? Aliascodeautomate { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Automate")]
        public int? Idautomate { get; set; }

        [Display(Name = "Analyse avec automate")]
        public bool Avecautomate { get; set; }

        [Display(Name = "Formuler à appliquer")]
        public string? Formuleautomate { get; set; }

        [Display(Name = "Afficher machine dans résultat")]
        public bool Affichermachineresultat { get; set; }

        [Display(Name = "Décimal après virgule : résultat standard")]
        public int? Decimalresultatstandard { get; set; }

        [Display(Name = "Décimal après virgule : résultat si")]
        public int? Decimalresultatsi { get; set; }

        [Display(Name = "Afficher méthode dans résultat")]
        public bool Affichermethodresultat { get; set; }

        public string? Commentaire { get; set; }

        [Display(Name = "Accrédité")]
        public bool Accredite { get; set; }

        public decimal Prix { get; set; }

        [Display(Name = "Active ?")]
        public bool Isactive { get; set; }

        [Display(Name = "Unité standard")]
        public string? Codeunite { get; set; } = null!;

        [Display(Name = "Unité si")]
        public string? Codeunitesi { get; set; } = null!;

        [Display(Name = "Facteur de conversion : de standard à si")]
        public decimal? Facteurconversionsi { get; set; }

        [Display(Name = "Ordre d'affichage")]
        public int? Ordreaffichage { get; set; }

        [Display(Name = "Nature d'échantillon")]
        public int? Idnatureechantillon { get; set; }

        [Display(Name = "Laboratoire")]
        public int Idlaboratoire { get; set; }

        [Display(Name = "Analyse parent")]
        public Guid? Idanalyseparent { get; set; }

        [Display(Name = "Indice de révision")]
        public string? Indicederev { get; set; }

        public string? Codification { get; set; }

        [Display(Name = "Méthode")]
        public string[] IdsMethode { get; set; }

    }

}
