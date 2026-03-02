using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt4Config
{
    public int Config { get; set; }

    public int? ValueInt { get; set; }

    public string? ValueStr { get; set; }
}
