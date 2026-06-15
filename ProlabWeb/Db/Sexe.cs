using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Sexe
{
    public string Codesexe { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<Preleveur> Preleveurs { get; set; } = new List<Preleveur>();
}
