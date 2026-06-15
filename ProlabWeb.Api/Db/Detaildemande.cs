using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Detaildemande
{
    public Guid Detaildemandeid { get; set; }

    public Guid? Categorieid { get; set; }

    public Guid? Idanalyse { get; set; }

    public Guid Entetedemandeid { get; set; }

    public decimal Prix { get; set; }

    public decimal Partassurance { get; set; }

    public decimal Partpatient { get; set; }

    public decimal Complement { get; set; }

    public decimal Net { get; set; }

    public virtual Categorie? Categorie { get; set; }

    public virtual ICollection<Detailfacture> Detailfactures { get; set; } = new List<Detailfacture>();

    public virtual Entetedemande Entetedemande { get; set; } = null!;

    public virtual Analyse? IdanalyseNavigation { get; set; }

    public virtual ICollection<Prelevement> Prelevements { get; set; } = new List<Prelevement>();
}
