using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Enteteresultat
{
    public Guid Enteteresultatid { get; set; }

    public Guid Entetedemandeid { get; set; }

    public Guid Idanalyse { get; set; }

    public DateTime Date { get; set; }

    public string? Interpretation { get; set; }

    public string Codesite { get; set; } = null!;

    public Guid Technicienid { get; set; }

    public string? Statut { get; set; }

    public Guid? Biologisteid { get; set; }

    public bool Validationtechnicien { get; set; }

    public bool? Validationbiologiste { get; set; }

    public virtual Utilisateur? Biologiste { get; set; }

    public virtual Site CodesiteNavigation { get; set; } = null!;

    public virtual ICollection<Detailresultat> Detailresultats { get; set; } = new List<Detailresultat>();

    public virtual Entetedemande Entetedemande { get; set; } = null!;

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;

    public virtual Utilisateur Technicien { get; set; } = null!;
}
