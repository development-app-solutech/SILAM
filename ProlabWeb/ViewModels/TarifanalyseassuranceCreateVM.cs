using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class TarifanalyseassuranceCreateVM
    {
        public string Codeassurance { get; set; } = null!;

        public int? Idlaboratoire { get; set; }

        public List<TarifAnalyseVM> Tarif { get; set; } = new List<TarifAnalyseVM>();
    }
}
