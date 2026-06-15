namespace ProlabWeb.Models
{
    public class MedicalResultsViewModel
    {
        public List<StatisticCard> Statistics { get; set; } = new List<StatisticCard>();
        public List<MedicalResult> Results { get; set; } = new List<MedicalResult>();
        public string Title { get; set; } = "Liste des 10 derniers résultats médicaux";
        public string CompanyInfo { get; set; } = "2025© Solutech Informatique et Services";
    }

    public class StatisticCard
    {
        public string Title { get; set; } = "";
        public string Count { get; set; } = "";
        public string IconClass { get; set; } = "";
        public string BackgroundColor { get; set; } = "";
        public string ButtonText { get; set; } = "Plus d'infos";
    }

    public class MedicalResult
    {
        public string NumeroCommande { get; set; } = "";
        public string Date { get; set; } = "";
        public string Patient { get; set; } = "";
        public string PatientId { get; set; } = "";
        public string Prescripteur { get; set; } = "";
    }
}
