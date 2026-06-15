using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ProlabWeb.ViewModels
{
    public class DetailMaterielApproVM
    {
        public Guid Materielid { get; set; }

        //public string Nom { get; set; } = null!;

        public decimal Quantite { get; set; }
    }
} 