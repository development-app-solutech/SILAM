using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Typedocumentidentite
{
    public string Codetypedocumentidentite { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
