using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class MaterielEditVM : MaterielCreateVM
    {
        [Required]
        public Guid Materielid { get; set; }
    }
} 