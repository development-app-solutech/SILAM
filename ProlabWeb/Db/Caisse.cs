using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Caisse
{
    public Guid Caisseid { get; set; }

    public bool Isactive { get; set; }

    public string Codesite { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public virtual ICollection<Affectationcaisse> Affectationcaisses { get; set; } = new List<Affectationcaisse>();

    public virtual ICollection<Caisseappro> Caisseappros { get; set; } = new List<Caisseappro>();

    public virtual ICollection<Caissedepense> Caissedepenses { get; set; } = new List<Caissedepense>();

    public virtual Site CodesiteNavigation { get; set; } = null!;

    public virtual ICollection<Entetefacture> Entetefactures { get; set; } = new List<Entetefacture>();
}
