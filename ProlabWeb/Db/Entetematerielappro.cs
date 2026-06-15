using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Entetematerielappro
{
    public Guid Entetematerielapproid { get; set; }

    public DateTime Date { get; set; }

    public string Numero { get; set; } = null!;

    public Guid Utilisateurid { get; set; }

    public Guid Fournisseurid { get; set; }

    public virtual ICollection<Detailmaterielappro> Detailmaterielappros { get; set; } = new List<Detailmaterielappro>();

    public virtual Fournisseur Fournisseur { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
