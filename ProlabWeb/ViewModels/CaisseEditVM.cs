using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class CaisseEditVM : CaisseCreateVM
    {
        [Required]
        public Guid Caisseid { get; set; }
    }
} 