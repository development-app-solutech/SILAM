using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Typeassurance
{
    public string Codetypeassurance { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public virtual ICollection<Assurance> Assurances { get; set; } = new List<Assurance>();
}
