using System;
using System.Collections.Generic;
using ProlabWeb.Db;

namespace ProlabWeb.ViewModels
{
    public class AnalysematerielIndexVM
    {
        public Guid Idanalyse { get; set; }
        public string AnalyseNom { get; set; }
        public List<Analysemateriel> Materiels { get; set; } = new List<Analysemateriel>();
    }
}
