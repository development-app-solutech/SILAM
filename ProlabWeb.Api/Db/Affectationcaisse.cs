using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Affectationcaisse
{
    public Guid Affectationcaisseid { get; set; }

    public Guid Caisseid { get; set; }

    public Guid Utilisateurid { get; set; }

    public DateTime Date { get; set; }

    public virtual Caisse Caisse { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
