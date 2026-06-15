using System;
using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.ViewModels
{
    public class MaterielCreateVM
    {
        [Required]
        public string Nom { get; set; }
        public string? Description { get; set; }
        [Required]
        public Guid Categoriematerielid { get; set; }
        [Required]
        public int Idlaboratoire { get; set; }
        public decimal Prix { get; set; }
        public decimal? Quantitemin { get; set; }
        public DateTime? Dateperemption { get; set; }
        public string? Zonestockage { get; set; }
        public string? Conditionstockage { get; set; }
        public string Codebarre { get; set; }
    }
} 