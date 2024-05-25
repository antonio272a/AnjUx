using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class NumericExtensions
    {
        public static bool IsInRange<T>(this T value, T minValue, T maxValue) where T : struct, IComparable<T>
        {
            return value.CompareTo(minValue) > 0 && value.CompareTo(maxValue) < 0;
        }
    }
}
