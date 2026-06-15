using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Signatureutilisateur
{
    public Guid Signatureutilisateurid { get; set; }

    public Guid Utilisateurid { get; set; }

    public byte[] Image { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
