using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class TarifanalyseassuranceCreateAvecOngletVM
    {
        public string Codeassurance { get; set; }

        public string Nom { get; set; }

        public string? Prix { get; set; }
    }
}
