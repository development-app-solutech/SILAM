using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Categorieanalyse
{
    public Guid Categorieid { get; set; }

    public Guid Idanalyse { get; set; }

    public Guid Categorieanalyseid { get; set; }

    public virtual Categorie Categorie { get; set; } = null!;

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;
}
