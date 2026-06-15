using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Parametre
{
    public Guid Parametreid { get; set; }

    public Guid Idanalyse { get; set; }

    public string Nom { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool Masquerdansrapport { get; set; }

    public string? Formuleautomate { get; set; }

    public string Codeunite { get; set; } = null!;

    public string Codeunitesi { get; set; } = null!;

    public int? Decimalresultatstandard { get; set; }

    public int? Decimalresultatsi { get; set; }

    public decimal? Facteurconversionsi { get; set; }

    public int Ordreaffichage { get; set; }

    public string? Valuebuilder { get; set; }

    public virtual Unite CodeuniteNavigation { get; set; } = null!;

    public virtual Unite CodeunitesiNavigation { get; set; } = null!;

    public virtual ICollection<Detailresultat> Detailresultats { get; set; } = new List<Detailresultat>();

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;
}
