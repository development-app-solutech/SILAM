using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Natureechantillon
{
    public int Idnatureechantillon { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<Analyse> Analyses { get; set; } = new List<Analyse>();

    public virtual ICollection<Prelevement> Prelevements { get; set; } = new List<Prelevement>();
}
