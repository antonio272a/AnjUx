using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Client
{
    public class CoreClientConfig
    {
        private static CoreClientConfig? _instance;

        public static CoreClientConfig Instance
        {
            get
            {
                _instance ??= new CoreClientConfig();
                return _instance;
            }
        }

        public string? BaseGenericSearchUrl { get; set; }
    }
}
