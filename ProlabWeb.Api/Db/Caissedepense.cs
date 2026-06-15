using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Caissedepense
{
    public Guid Caissedepenseid { get; set; }

    public Guid Caisseid { get; set; }

    public decimal Montant { get; set; }

    public string? Motif { get; set; }

    public virtual Caisse Caisse { get; set; } = null!;
}
