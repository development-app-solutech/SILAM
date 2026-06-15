using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Demandeanalysemateriel
{
    public Guid Demandeanalysematerielid { get; set; }

    public Guid Entetedemandeid { get; set; }

    public Guid Analysematerielid { get; set; }

    public decimal Quantite { get; set; }

    public virtual Analysemateriel Analysemateriel { get; set; } = null!;

    public virtual Entetedemande Entetedemande { get; set; } = null!;
}
