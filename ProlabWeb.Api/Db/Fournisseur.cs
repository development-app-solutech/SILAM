using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Fournisseur
{
    public Guid Fournisseurid { get; set; }

    public string Nom { get; set; } = null!;

    public string? Adresse { get; set; }

    public string? Tel { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Entetematerielappro> Entetematerielappros { get; set; } = new List<Entetematerielappro>();
}
