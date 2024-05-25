using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class IDictionaryExtensions
    {
        public static T? ValueIfKeyExists<K, T>(this IDictionary<K, T> dicionario, K chave)
        {
            return dicionario.TryGetValue(chave, out T? value) ? value : default;
        }
    }
}
