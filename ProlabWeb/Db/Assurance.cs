using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Assurance
{
    public string Codeassurance { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public string Codetypeassurance { get; set; } = null!;

    public virtual Typeassurance CodetypeassuranceNavigation { get; set; } = null!;

    public virtual ICollection<Policeassurance> Policeassurances { get; set; } = new List<Policeassurance>();

    public virtual ICollection<Tarifanalyseassurance> Tarifanalyseassurances { get; set; } = new List<Tarifanalyseassurance>();

    public virtual ICollection<Tarifcategorieassurance> Tarifcategorieassurances { get; set; } = new List<Tarifcategorieassurance>();
}
