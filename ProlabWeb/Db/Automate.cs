using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Automate
{
    public int Idautomate { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public string Modeauto { get; set; } = null!;

    public string Confentree { get; set; } = null!;

    public string? Processconfig { get; set; }

    public string? Parserconfig { get; set; }

    public string Confsortie { get; set; } = null!;

    public string? Note { get; set; }

    public virtual ICollection<Analyse> Analyses { get; set; } = new List<Analyse>();
}
