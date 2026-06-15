using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Loboratoire
{
    public int Idlaboratoire { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<Analyse> Analyses { get; set; } = new List<Analyse>();

    public virtual ICollection<Materiel> Materiels { get; set; } = new List<Materiel>();

    public virtual ICollection<Utilisateurlaboratoire> Utilisateurlaboratoires { get; set; } = new List<Utilisateurlaboratoire>();
}
