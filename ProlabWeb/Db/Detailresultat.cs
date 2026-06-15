using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Detailresultat
{
    public Guid Detailresultatid { get; set; }

    public Guid Enteteresultatid { get; set; }

    public Guid Parametreid { get; set; }

    public DateTime Date { get; set; }

    public string? Commentaire { get; set; }

    public string? Databuilder { get; set; }

    public string? Resultat { get; set; }

    public string? Resultatsi { get; set; }

    public virtual Enteteresultat Enteteresultat { get; set; } = null!;

    public virtual Parametre Parametre { get; set; } = null!;
}
