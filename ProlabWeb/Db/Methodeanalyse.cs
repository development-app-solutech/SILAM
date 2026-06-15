using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Methodeanalyse
{
    public Guid Idanalyse { get; set; }

    public int Idmethode { get; set; }

    public bool Isdefaultmethode { get; set; }

    public Guid Methodeanalyseid { get; set; }

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;

    public virtual Methode IdmethodeNavigation { get; set; } = null!;
}
