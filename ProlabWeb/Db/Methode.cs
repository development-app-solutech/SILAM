using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Methode
{
    public int Idmethode { get; set; }

    public string Nom { get; set; } = null!;

    public bool Isactive { get; set; }

    public virtual ICollection<Methodeanalyse> Methodeanalyses { get; set; } = new List<Methodeanalyse>();
}
