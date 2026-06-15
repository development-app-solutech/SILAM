using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Typepeau
{
    public string Codetypepeau { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
