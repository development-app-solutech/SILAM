using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Tarifanalyseassurance
{
    public Guid Tarifanalyseassuranceid { get; set; }

    public Guid Idanalyse { get; set; }

    public string Codeassurance { get; set; } = null!;

    public decimal Prix { get; set; }

    public virtual Assurance CodeassuranceNavigation { get; set; } = null!;

    public virtual Analyse IdanalyseNavigation { get; set; } = null!;
}
