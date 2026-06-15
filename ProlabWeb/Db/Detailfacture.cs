using System;
using System.Collections.Generic;

namespace ProlabWeb.Db;

public partial class Detailfacture
{
    public Guid Detailfactureid { get; set; }

    public Guid Entetefactureid { get; set; }

    public Guid Detaildemandeid { get; set; }

    public virtual Detaildemande Detaildemande { get; set; } = null!;

    public virtual Entetefacture Entetefacture { get; set; } = null!;
}
