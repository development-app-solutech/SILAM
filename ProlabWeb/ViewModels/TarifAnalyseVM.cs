using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class TarifAnalyseVM
    {
        public Guid Idanalyse { get; set; }

        public string Nom { get; set; } = null!;

        public string? Prix { get; set; }
    }
}
