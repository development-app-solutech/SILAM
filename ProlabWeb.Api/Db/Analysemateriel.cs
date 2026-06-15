using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Analysemateriel
{
    public Guid Analysematerielid { get; set; }

    public Guid Idanalyse { get; set; }

    public Guid Materielid { get; set; }

    public decimal Quantite { get; set; }

    public virtual ICollection<Demandeanalysemateriel> Demandeanalysemateriels { get; set; } = new List<Demandeanalysemateriel>();

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;

    public virtual Materiel Materiel { get; set; } = null!;
}
