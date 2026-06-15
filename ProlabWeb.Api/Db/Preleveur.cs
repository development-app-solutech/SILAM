using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Preleveur
{
    public Guid Preleveurid { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Codesexe { get; set; } = null!;

    public DateOnly Datenaissance { get; set; }

    public string? Tel1 { get; set; }

    public string? Tel2 { get; set; }

    public string Mob1 { get; set; } = null!;

    public string? Mob2 { get; set; }

    public string? Email { get; set; }

    public string Fonction { get; set; } = null!;

    public virtual Sexe CodesexeNavigation { get; set; } = null!;

    public virtual ICollection<Prelevement> Prelevements { get; set; } = new List<Prelevement>();
}
