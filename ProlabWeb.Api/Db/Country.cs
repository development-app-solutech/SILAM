using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Country
{
    public string Countryisocode2 { get; set; } = null!;

    public string Countryisocode3 { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Currencycode { get; set; }

    public string? Internetdomain { get; set; }

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
