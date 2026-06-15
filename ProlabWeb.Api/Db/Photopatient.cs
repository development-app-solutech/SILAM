using System;
using System.Collections.Generic;

namespace ProlabWeb.Api.Db;

public partial class Photopatient
{
    public Guid Photopatientid { get; set; }

    public Guid Patientid { get; set; }

    public byte[] Photo { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
