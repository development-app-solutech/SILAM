using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Categorie
{
    public Guid Categorieid { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Prix { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<Categorieanalyse> Categorieanalyses { get; set; } = new List<Categorieanalyse>();

    public virtual ICollection<Detaildemande> Detaildemandes { get; set; } = new List<Detaildemande>();

    public virtual ICollection<Tarifcategorieassurance> Tarifcategorieassurances { get; set; } = new List<Tarifcategorieassurance>();
}
