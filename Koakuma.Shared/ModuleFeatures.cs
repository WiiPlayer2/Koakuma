using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koakuma.Shared
{
    [Flags]
    public enum ModuleFeatures
    {
        Default = 0x00,
        Service = 0x01,
    }
}
