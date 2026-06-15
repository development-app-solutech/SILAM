using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Prescripteur
{
    public Guid Prescripteurid { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public string? Adresse { get; set; }

    public string? Tel { get; set; }

    public virtual ICollection<Entetedemande> Entetedemandes { get; set; } = new List<Entetedemande>();
}
