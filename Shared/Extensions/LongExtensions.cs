using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class LongExtensions
    {
        public static bool IsNullOrSmallerThanZero(this long? value)
        {
            return !value.HasValue || value.Value <= 0;
        }
    }
}
