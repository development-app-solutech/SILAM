using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Utilisateurprofil
{
    public Guid Utilisateurprofilid { get; set; }

    public Guid Utilisateurid { get; set; }

    public string Profilid { get; set; } = null!;

    public virtual Profil Profil { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
