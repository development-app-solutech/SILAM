namespace ProlabWeb
{
    public enum EnumStatutDemande
    {
        NonTraite,     // Results not ready yet
        EnCours,       // Partially ready (some analyses done)
        Traite         // All results ready
    }

    public static class EnumStatutDemandeExtensions
    {
        public static string ToDisplayString(this EnumStatutDemande status)
        {
            return status switch
            {
                EnumStatutDemande.NonTraite => "En attente de saisie",
                EnumStatutDemande.EnCours => "En cours de saisie",
                EnumStatutDemande.Traite => "Résultats saisie",
                _ => "Inconnu"
            };
        }

        public static string ToCssClass(this EnumStatutDemande status)
        {
            return status switch
            {
                EnumStatutDemande.NonTraite => "kt-badge kt-badge-destructive kt-badge-outline rounded-[30px]",
                EnumStatutDemande.EnCours => "kt-badge kt-badge-primary kt-badge-outline rounded-[30px]",
                EnumStatutDemande.Traite => "kt-badge kt-badge-success kt-badge-outline rounded-[30px]",
                _ => "kt-badge kt-badge-secondary kt-badge-outline rounded-[30px]"
            };
        }
    }
}
