using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Analyse
{
    public Guid Idanalyse { get; set; }

    public string? Codeparametre { get; set; }

    public string? Aliascodeautomate { get; set; }

    public string? Description { get; set; }

    public int? Idautomate { get; set; }

    public bool Avecautomate { get; set; }

    public string? Formuleautomate { get; set; }

    public bool Affichermachineresultat { get; set; }

    public int? Decimalresultatstandard { get; set; }

    public int? Decimalresultatsi { get; set; }

    public bool Affichermethodresultat { get; set; }

    public string? Commentaire { get; set; }

    public bool Accredite { get; set; }

    public decimal Prix { get; set; }

    public bool Isactive { get; set; }

    public string? Codeunite { get; set; }

    public string? Codeunitesi { get; set; }

    public decimal? Facteurconversionsi { get; set; }

    public int? Ordreaffichage { get; set; }

    public int? Idnatureechantillon { get; set; }

    public int Idlaboratoire { get; set; }

    public Guid? Idanalyseparent { get; set; }

    public string? Indicederev { get; set; }

    public string? Codification { get; set; }

    public string Nom { get; set; } = null!;

    public virtual ICollection<Analysemateriel> Analysemateriels { get; set; } = new List<Analysemateriel>();

    public virtual ICollection<Categorieanalyse> Categorieanalyses { get; set; } = new List<Categorieanalyse>();

    public virtual Unite? CodeuniteNavigation { get; set; }

    public virtual Unite? CodeunitesiNavigation { get; set; }

    public virtual ICollection<Detaildemande> Detaildemandes { get; set; } = new List<Detaildemande>();

    public virtual ICollection<Enteteresultat> Enteteresultats { get; set; } = new List<Enteteresultat>();

    public virtual Analyse? IdanalyseparentNavigation { get; set; }

    public virtual Automate? IdautomateNavigation { get; set; }

    public virtual Loboratoire IdlaboratoireNavigation { get; set; } = null!;

    public virtual Natureechantillon? IdnatureechantillonNavigation { get; set; }

    public virtual ICollection<Analyse> InverseIdanalyseparentNavigation { get; set; } = new List<Analyse>();

    public virtual ICollection<Methodeanalyse> Methodeanalyses { get; set; } = new List<Methodeanalyse>();

    public virtual ICollection<Parametre> Parametres { get; set; } = new List<Parametre>();

    public virtual ICollection<Tarifanalyseassurance> Tarifanalyseassurances { get; set; } = new List<Tarifanalyseassurance>();

    public virtual ICollection<Valeurreference> Valeurreferences { get; set; } = new List<Valeurreference>();
}
