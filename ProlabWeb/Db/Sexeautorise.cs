using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Sexeautorise
{
    public string Sexeautorisecode { get; set; } = null!;

    public string? Valeur { get; set; }

    public virtual ICollection<Valeurreference> Valeurreferences { get; set; } = new List<Valeurreference>();
}
