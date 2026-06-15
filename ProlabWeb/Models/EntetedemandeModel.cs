namespace ProlabWeb.Models
{
    public class EntetedemandeModel
    {
        public Guid Entetedemandeid { get; set; }

        public string Nom { get; set; }

        public string Prenom { get; set; }

        public string Sexe { get; set; } = null!;

        public DateTime Datenaissance { get; set; }

        public string Analyses { get; set; } = null!;

        public string Prescripteur { get; set; }

        public string Assurance { get; set; }

        public string Taux { get; set; }

        public string Numero { get; set; } = null!;

        public DateTime Date { get; set; }

        public EnumStatutDemande Statut { get; set; }

        public string StatutDisplay => Statut.ToDisplayString();

        public string StatutCssClass => Statut.ToCssClass();

    }
}
