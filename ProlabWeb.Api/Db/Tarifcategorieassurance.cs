using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Tarifcategorieassurance
{
    public Guid Tarifcategorieassuranceid { get; set; }

    public Guid Categorieid { get; set; }

    public string Codeassurance { get; set; } = null!;

    public decimal Prix { get; set; }

    public virtual Categorie Categorie { get; set; } = null!;

    public virtual Assurance CodeassuranceNavigation { get; set; } = null!;
}
