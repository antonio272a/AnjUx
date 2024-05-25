using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static Type NonNullableType(this PropertyInfo minhaPropriedade)
        {
            var tipoPropriedade = minhaPropriedade.PropertyType;

            if (tipoPropriedade.IsGenericType && tipoPropriedade.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var tipoNaoNulo = minhaPropriedade.PropertyType.GetGenericArguments()[0];

                return tipoNaoNulo;
            }
            else
            {
                return minhaPropriedade.PropertyType;
            }
        }

        public static bool IsListOfEnum(this Type tipo)
        {
            // Checa se é uma List<>
            if (tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Se é uma Lista, verifica os elementos
                Type listType = tipo.GetGenericArguments()[0];

                // Checa se o elemento é um Enum
                if (listType.IsEnum)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
