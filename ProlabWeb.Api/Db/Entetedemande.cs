using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Entetedemande
{
    public Guid Entetedemandeid { get; set; }

    public Guid Patientid { get; set; }

    public Guid Prescripteurid { get; set; }

    public Guid? Policeassuranceid { get; set; }

    public string Numero { get; set; } = null!;

    public string Codesite { get; set; } = null!;

    public Guid Utilisateurid { get; set; }

    public int Ordre { get; set; }

    public DateTime Date { get; set; }

    public Guid? Partenaireid { get; set; }

    public virtual Site CodesiteNavigation { get; set; } = null!;

    public virtual ICollection<Demandeanalysemateriel> Demandeanalysemateriels { get; set; } = new List<Demandeanalysemateriel>();

    public virtual ICollection<Detaildemande> Detaildemandes { get; set; } = new List<Detaildemande>();

    public virtual ICollection<Entetefacture> Entetefactures { get; set; } = new List<Entetefacture>();

    public virtual ICollection<Enteteresultat> Enteteresultats { get; set; } = new List<Enteteresultat>();

    public virtual Partenaire? Partenaire { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Policeassurance? Policeassurance { get; set; }

    public virtual Prescripteur Prescripteur { get; set; } = null!;

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
