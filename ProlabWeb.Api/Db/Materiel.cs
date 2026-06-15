using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Materiel
{
    public Guid Materielid { get; set; }

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public Guid Categoriematerielid { get; set; }

    public int Idlaboratoire { get; set; }

    public decimal Prix { get; set; }

    public decimal? Quantitemin { get; set; }

    public DateTime? Dateperemption { get; set; }

    public string? Zonestockage { get; set; }

    public string? Conditionstockage { get; set; }

    public string Codebarre { get; set; } = null!;

    public virtual ICollection<Analysemateriel> Analysemateriels { get; set; } = new List<Analysemateriel>();

    public virtual Categoriemateriel Categoriemateriel { get; set; } = null!;

    public virtual ICollection<Detailmaterielappro> Detailmaterielappros { get; set; } = new List<Detailmaterielappro>();

    public virtual Loboratoire IdlaboratoireNavigation { get; set; } = null!;
}
