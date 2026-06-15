using Microsoft.AspNetCore.Mvc;
using ProlabWeb.Models;

namespace ProlabWeb.Controllers
{
    public class MedicalResultsController : Controller
    {
        public IActionResult Index()
        {
            var model = new MedicalResultsViewModel
            {
                Title = "Liste des 10 derniers résultats médicaux",
                CompanyInfo = "2025© Solutech Informatique et Services",
                Statistics = new List<StatisticCard>
                {
                    new StatisticCard
                    {
                        Title = "En Attente",
                        Count = "5",
                        IconClass = "fas fa-clock",
                        BackgroundColor = "warning",
                        ButtonText = "Plus d'infos"
                    },
                    new StatisticCard
                    {
                        Title = "Résultats",
                        Count = "3",
                        IconClass = "fas fa-flask",
                        BackgroundColor = "success",
                        ButtonText = "Plus d'infos"
                    },
                    new StatisticCard
                    {
                        Title = "Validations",
                        Count = "2",
                        IconClass = "fas fa-check-circle",
                        BackgroundColor = "info",
                        ButtonText = "Plus d'infos"
                    }
                },
                Results = new List<MedicalResult>
                {
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000051",
                        Date = "20/08/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25667400003B",
                        Date = "25/07/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25667400003A",
                        Date = "24/07/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000050",
                        Date = "19/08/2025",
                        Patient = "marie dubois",
                        PatientId = "2847593621",
                        Prescripteur = "Dr. Martin"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000049",
                        Date = "18/08/2025",
                        Patient = "jean leclerc",
                        PatientId = "3946728194",
                        Prescripteur = "Dr. Durand"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000048",
                        Date = "17/08/2025",
                        Patient = "sophie bernard",
                        PatientId = "5847392018",
                        Prescripteur = "Dr. Rousseau"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000047",
                        Date = "16/08/2025",
                        Patient = "pierre moreau",
                        PatientId = "7394852647",
                        Prescripteur = "Dr. Vincent"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000046",
                        Date = "15/08/2025",
                        Patient = "claire petit",
                        PatientId = "8293847562",
                        Prescripteur = "Dr. Lambert"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000045",
                        Date = "14/08/2025",
                        Patient = "antoine fournier",
                        PatientId = "9485739261",
                        Prescripteur = "Dr. Garcia"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000044",
                        Date = "13/08/2025",
                        Patient = "isabelle girard",
                        PatientId = "1847392847",
                        Prescripteur = "Dr. Lefebvre"
                    }
                }
            };

            return View(model);
        }

        /// <summary>
        /// API endpoint to get medical results data (for AJAX calls)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetMedicalResults()
        {
            var results = new List<MedicalResult>
            {
                new MedicalResult
                {
                    NumeroCommande = "25CS001000051",
                    Date = "20/08/2025",
                    Patient = "akakpo komlan",
                    PatientId = "1753357480",
                    Prescripteur = "Saffa"
                },
                new MedicalResult
                {
                    NumeroCommande = "25667400003B",
                    Date = "25/07/2025",
                    Patient = "akakpo komlan",
                    PatientId = "1753357480",
                    Prescripteur = "Saffa"
                }
            };

            return Json(results);
        }

        /// <summary>
        /// Simple responsive datatable demo
        /// </summary>
        /// <returns></returns>
        public IActionResult Simple()
        {
            var model = new MedicalResultsViewModel
            {
                Title = "Liste des 10 derniers résultats médicaux",
                CompanyInfo = "2025© Solutech Informatique et Services",
                Results = new List<MedicalResult>
                {
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000051",
                        Date = "20/08/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25667400003B",
                        Date = "25/07/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25667400003A",
                        Date = "24/07/2025",
                        Patient = "akakpo komlan",
                        PatientId = "1753357480",
                        Prescripteur = "Saffa"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000050",
                        Date = "19/08/2025",
                        Patient = "marie dubois",
                        PatientId = "2847593621",
                        Prescripteur = "Dr. Martin"
                    },
                    new MedicalResult
                    {
                        NumeroCommande = "25CS001000049",
                        Date = "18/08/2025",
                        Patient = "jean leclerc",
                        PatientId = "3946728194",
                        Prescripteur = "Dr. Durand"
                    }
                }
            };

            return View(model);
        }

        /// <summary>
        /// API endpoint to get statistics data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetStatistics()
        {
            var stats = new List<StatisticCard>
            {
                new StatisticCard
                {
                    Title = "En Attente",
                    Count = "5",
                    IconClass = "fas fa-clock",
                    BackgroundColor = "warning"
                },
                new StatisticCard
                {
                    Title = "Résultats",
                    Count = "3",
                    IconClass = "fas fa-flask",
                    BackgroundColor = "success"
                },
                new StatisticCard
                {
                    Title = "Validations",
                    Count = "2",
                    IconClass = "fas fa-check-circle",
                    BackgroundColor = "info"
                }
            };

            return Json(stats);
        }
    }
}
