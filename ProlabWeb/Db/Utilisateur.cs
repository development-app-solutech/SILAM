using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Utilisateur
{
    public Guid Utilisateurid { get; set; }

    public string Matricule { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Codesexe { get; set; } = null!;

    public DateOnly Datenaissance { get; set; }

    public string? Nationnalite { get; set; }

    public string? Tel1 { get; set; }

    public string? Tel2 { get; set; }

    public string Mob1 { get; set; } = null!;

    public string? Mob2 { get; set; }

    public string? Email { get; set; }

    public bool Isactive { get; set; }

    public bool Mustchangepass { get; set; }

    public string? Codesite { get; set; }

    public string Login { get; set; } = null!;

    public string? Userid { get; set; }

    public DateOnly? Creationdate { get; set; }

    public string? Createdby { get; set; }

    public DateOnly? Updatedate { get; set; }

    public string? Updateby { get; set; }

    public string Password { get; set; } = null!;

    public string? Idinh { get; set; }

    public virtual ICollection<Affectationcaisse> Affectationcaisses { get; set; } = new List<Affectationcaisse>();

    public virtual Site? CodesiteNavigation { get; set; }

    public virtual ICollection<Entetedemande> Entetedemandes { get; set; } = new List<Entetedemande>();

    public virtual ICollection<Entetefacture> Entetefactures { get; set; } = new List<Entetefacture>();

    public virtual ICollection<Entetematerielappro> Entetematerielappros { get; set; } = new List<Entetematerielappro>();

    public virtual ICollection<Enteteresultat> EnteteresultatBiologistes { get; set; } = new List<Enteteresultat>();

    public virtual ICollection<Enteteresultat> EnteteresultatTechniciens { get; set; } = new List<Enteteresultat>();

    public virtual Country? NationnaliteNavigation { get; set; }

    public virtual ICollection<Signatureutilisateur> Signatureutilisateurs { get; set; } = new List<Signatureutilisateur>();

    public virtual ICollection<Utilisateurlaboratoire> Utilisateurlaboratoires { get; set; } = new List<Utilisateurlaboratoire>();

    public virtual ICollection<Utilisateurprofil> Utilisateurprofils { get; set; } = new List<Utilisateurprofil>();
}
