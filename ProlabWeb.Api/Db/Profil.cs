using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Profil
{
    public string Profilid { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public bool Isactive { get; set; }

    public virtual ICollection<Utilisateurprofil> Utilisateurprofils { get; set; } = new List<Utilisateurprofil>();
}
