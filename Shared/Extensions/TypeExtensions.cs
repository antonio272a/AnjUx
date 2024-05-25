using AnjUx.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsHumanPrimitive(this Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null) return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return IsNumericType(Nullable.GetUnderlyingType(type)!);
                    return false;
            }

            return false;
        }

        public static bool IsNullable(this Type type)
        {
            if (type.Name == "Nullable`1")
                return true;

            return false;
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static Type NonNullableType(this Type meuTipo)
        {
            if (meuTipo.IsGenericType && meuTipo.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var tipoNaoNulo = meuTipo.GetGenericArguments()[0];

                return tipoNaoNulo;
            }
            else
            {
                return meuTipo;
            }
        }

        /// <summary>
        ///     Verifica se o tipo informado pode ser inferido a partir da lista de tipos informados
        /// </summary>
        /// <param name="type"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool IsAssignableFrom(this Type type, params Type[] types)
        {
            // Irá determinar se é assignable ou não
            bool isAssignable = false;

            // Só entra se tiver o tipo definido e se houver algum tipo especificado para comparar
            if (type != null && types != null && types.Length > 0)
                foreach (Type t in types)
                    if (type.IsAssignableFrom(t))
                    {
                        isAssignable = true;
                        break;
                    }

            // Retorno geral
            return isAssignable;
        }

        public static List<PropertyInfo> Properties(this Type tipo, Type? tipoAtributo = null, bool excluirTipoEspecificado = false)
        {
            List<PropertyInfo> props = [.. tipo.GetProperties()];

            if (tipoAtributo != null && !excluirTipoEspecificado) props = props.Where(prop => prop.GetCustomAttribute(tipoAtributo!) != null).ToList();

            if (tipoAtributo != null && excluirTipoEspecificado) props = props.Where(prop => prop.GetCustomAttribute(tipoAtributo!) == null).ToList();

            return props;
        }

        /// <summary>
        /// Pega o nome da Tabela no Banco de Dados
        /// </summary>
        /// <param name="tipo">Classe em questão</param>
        /// <returns>Retorna o TableName do DBTableAttribute</returns>
        public static string GetDBTableFullName(this Type tipo)
        {
            // vars e o DBTable da classe, se existir
            var fullName = string.Empty;
            var att = tipo.GetCustomAttribute<DBTableAttribute>();

            // Se existir o DBTable, usa o nome da tabela indicado, caso contrário usa o nome da própria classe
            if (att != null)
                fullName = $"{att.Table}";
            else
                fullName = tipo.Name;

            return fullName;
        }

    }
}
