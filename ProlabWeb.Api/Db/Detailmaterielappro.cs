using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Detailmaterielappro
{
    public Guid Detailmaterielapproid { get; set; }

    public Guid Entetematerielapproid { get; set; }

    public Guid Materielid { get; set; }

    public decimal Quantite { get; set; }

    public virtual Entetematerielappro Entetematerielappro { get; set; } = null!;

    public virtual Materiel Materiel { get; set; } = null!;
}
