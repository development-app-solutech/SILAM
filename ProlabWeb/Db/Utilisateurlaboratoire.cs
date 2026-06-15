using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Utilisateurlaboratoire
{
    public Guid Utilisateurlaboratoireid { get; set; }

    public Guid Utilisateurid { get; set; }

    public int Idlaboratoire { get; set; }

    public virtual Loboratoire IdlaboratoireNavigation { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
