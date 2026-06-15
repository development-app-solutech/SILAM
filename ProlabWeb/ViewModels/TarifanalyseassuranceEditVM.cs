using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class TarifanalyseassuranceEditVM
    {
        public Guid Tarifanalyseassuranceid { get; set; }

        public string Codeassurance { get; set; } = null!;

        public int? Idlaboratoire { get; set; }

        public List<TarifAnalyseVM> Tarif { get; set; } = new List<TarifAnalyseVM>();
    }
}
