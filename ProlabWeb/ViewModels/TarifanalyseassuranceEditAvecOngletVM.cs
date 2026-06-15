using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class TarifanalyseassuranceEditAvecOngletVM
    {
        public Guid? Tarifanalyseassuranceid { get; set; }

        public Guid Idanalyse { get; set; }

        public string Codeassurance { get; set; } = null!;

        public string Nom { get; set; } = null!;

        public string? Prix { get; set; }
    }
}
