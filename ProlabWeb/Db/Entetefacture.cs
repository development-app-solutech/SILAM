using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Entetefacture
{
    public Guid Entetefactureid { get; set; }

    public Guid Entetedemandeid { get; set; }

    public Guid Caisseid { get; set; }

    public Guid Utilisateurid { get; set; }

    public DateTime Date { get; set; }

    public string Numero { get; set; } = null!;

    public virtual Caisse Caisse { get; set; } = null!;

    public virtual ICollection<Detailfacture> Detailfactures { get; set; } = new List<Detailfacture>();

    public virtual Entetedemande Entetedemande { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
