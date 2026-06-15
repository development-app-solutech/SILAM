using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Patient
{
    public Guid Patientid { get; set; }

    public string Codesite { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string? Nomusage { get; set; }

    public string Codesexe { get; set; } = null!;

    public DateTime Datenaissance { get; set; }

    public string? Codetypepeau { get; set; }

    public string Codetypedocumentidentite { get; set; } = null!;

    public string? Numerodocumentidentite { get; set; }

    public string? Ville { get; set; }

    public string? Quartier { get; set; }

    public string? Renseignementclinique { get; set; }

    public string Lieunaissance { get; set; } = null!;

    public string? Tel { get; set; }

    public string? Adresse { get; set; }

    public string? Email { get; set; }

    public string Code { get; set; } = null!;

    public DateTime? Creationdate { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Updatedate { get; set; }

    public string? Updateby { get; set; }

    public virtual Sexe CodesexeNavigation { get; set; } = null!;

    public virtual Site CodesiteNavigation { get; set; } = null!;

    public virtual Typedocumentidentite CodetypedocumentidentiteNavigation { get; set; } = null!;

    public virtual Typepeau? CodetypepeauNavigation { get; set; }

    public virtual ICollection<Entetedemande> Entetedemandes { get; set; } = new List<Entetedemande>();

    public virtual ICollection<Photopatient> Photopatients { get; set; } = new List<Photopatient>();
}
