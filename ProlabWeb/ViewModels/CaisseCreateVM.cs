using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class CaisseCreateVM
    {
        [Required]
        public string Nom { get; set; }
        [Required]
        public string Codesite { get; set; }
        public bool Isactive { get; set; } = true;
        // Pour la liste déroulante des sites
        public List<SiteItem> Sites { get; set; } = new List<SiteItem>();
    }

    public class SiteItem
    {
        public string Codesite { get; set; }
        public string? Name { get; set; }
    }
} 