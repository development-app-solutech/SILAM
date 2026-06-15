using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class DashboardViewModel
    {
        public int PatientsCount { get; set; }
        public int DemandesCount { get; set; }
        public int ResultatsCount { get; set; }
        public int ValidationsCount { get; set; }
        public List<Enteteresultat> DerniersResultats { get; set; } = new List<Enteteresultat>();
    }
}
