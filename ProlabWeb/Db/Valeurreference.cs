using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Valeurreference
{
    public Guid Valeurreferenceid { get; set; }

    public Guid Idanalyse { get; set; }

    public string Sexeautorisecode { get; set; } = null!;

    public int? Agedebut { get; set; }

    public int? Agefin { get; set; }

    public string? Codeunitagedebut { get; set; }

    public string? Codeunitagefin { get; set; }

    public decimal? Referencefromvalue { get; set; }

    public decimal? Referencetovalue { get; set; }

    public string? Codeunitreference { get; set; }

    public decimal? Referencefromvaluesi { get; set; }

    public decimal? Referencetovaluesi { get; set; }

    public string? Codeunitreferencesi { get; set; }

    public string? Titre { get; set; }

    public virtual Unite? CodeunitagedebutNavigation { get; set; }

    public virtual Unite? CodeunitagefinNavigation { get; set; }

    public virtual Unite? CodeunitreferenceNavigation { get; set; }

    public virtual Unite? CodeunitreferencesiNavigation { get; set; }

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;

    public virtual Sexeautorise SexeautorisecodeNavigation { get; set; } = null!;
}
