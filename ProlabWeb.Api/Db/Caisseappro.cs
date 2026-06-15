using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Caisseappro
{
    public Guid Caisseapproid { get; set; }

    public Guid Caisseid { get; set; }

    public DateTime Date { get; set; }

    public decimal Montant { get; set; }

    public virtual Caisse Caisse { get; set; } = null!;
}
