using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Categoriemateriel
{
    public Guid Categoriematerielid { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<Materiel> Materiels { get; set; } = new List<Materiel>();
}
