using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Policeassurance
{
    public Guid Policeassuranceid { get; set; }

    public string Codeassurance { get; set; } = null!;

    public string Libelle { get; set; } = null!;

    public decimal Taux { get; set; }

    public virtual Assurance CodeassuranceNavigation { get; set; } = null!;

    public virtual ICollection<Entetedemande> Entetedemandes { get; set; } = new List<Entetedemande>();
}
