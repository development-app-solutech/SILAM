using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Unite
{
    public string Code { get; set; } = null!;

    public string? Name { get; set; }

    public bool? Isage { get; set; }

    public int? Isageday { get; set; }

    public virtual ICollection<Analyse> AnalyseCodeuniteNavigations { get; set; } = new List<Analyse>();

    public virtual ICollection<Analyse> AnalyseCodeunitesiNavigations { get; set; } = new List<Analyse>();

    public virtual ICollection<Parametre> ParametreCodeuniteNavigations { get; set; } = new List<Parametre>();

    public virtual ICollection<Parametre> ParametreCodeunitesiNavigations { get; set; } = new List<Parametre>();

    public virtual ICollection<Valeurreference> ValeurreferenceCodeunitagedebutNavigations { get; set; } = new List<Valeurreference>();

    public virtual ICollection<Valeurreference> ValeurreferenceCodeunitagefinNavigations { get; set; } = new List<Valeurreference>();

    public virtual ICollection<Valeurreference> ValeurreferenceCodeunitreferenceNavigations { get; set; } = new List<Valeurreference>();

    public virtual ICollection<Valeurreference> ValeurreferenceCodeunitreferencesiNavigations { get; set; } = new List<Valeurreference>();
}
