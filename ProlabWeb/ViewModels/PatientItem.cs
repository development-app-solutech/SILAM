using ProlabWeb.Db;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class PatientItem
    {
        public Patient Patient { get; set; }

        public Photopatient Photopatient { get; set; }
    }
}
