using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Site
{
    public string Codesite { get; set; } = null!;

    public string? Name { get; set; }

    public virtual ICollection<Caisse> Caisses { get; set; } = new List<Caisse>();

    public virtual ICollection<Entetedemande> Entetedemandes { get; set; } = new List<Entetedemande>();

    public virtual ICollection<Enteteresultat> Enteteresultats { get; set; } = new List<Enteteresultat>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<Utilisateur> Utilisateurs { get; set; } = new List<Utilisateur>();
}
