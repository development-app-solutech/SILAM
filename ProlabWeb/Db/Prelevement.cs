using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Prelevement
{
    public Guid Prelevementid { get; set; }

    public Guid Detaildemandeid { get; set; }

    public Guid Preleveurid { get; set; }

    public int Idnatureechantillon { get; set; }

    public DateTime Dateprelevement { get; set; }

    public string Statut { get; set; } = null!;

    public DateTime? Datereception { get; set; }

    public string? Siteanatomique { get; set; }

    public string? Renseignementclinique { get; set; }

    public string? Lieuprelevement { get; set; }

    public string? Lieureception { get; set; }

    public virtual Detaildemande Detaildemande { get; set; } = null!;

    public virtual Natureechantillon IdnatureechantillonNavigation { get; set; } = null!;

    public virtual Preleveur Preleveur { get; set; } = null!;
}
