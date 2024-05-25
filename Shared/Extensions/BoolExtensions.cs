using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class BoolExtensions
    {
        public static bool True(this bool? booleano)
        {
            return booleano != null && (bool)booleano!;
        }

        public static bool False(this bool? booleano)
        {
            return booleano != null || !((bool)booleano!);
        }
    }
}
