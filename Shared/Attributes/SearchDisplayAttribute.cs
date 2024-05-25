using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SearchDisplayAttribute : Attribute
    {
        public int Order { get; }

        public SearchDisplayAttribute(int order = -1)
        {
            Order = order;
        }
    }
}
