using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Interfaces
{
    public interface IActiveModel : IDbModel
    {
        public bool? Active { get; set; }
    }
}
