using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class ObjectExtensions
    {
        public static T? GetAttribute<T>(this object obj) where T : Attribute
        {
            return obj.GetType().GetCustomAttribute<T>();
        }

        public static Dictionary<string, object?> ToDictionary(this object source)
        {
            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            return properties.ToDictionary(prop => prop.Name, prop => prop.GetValue(source, null));
        }

        public static T? Clone<T>(this T source) where T : class, new()
        {
            if (source != null)
            {
                //Tratamento para prováveis loops Ex: Pessoa.Emails[0].Pessoa
                var s = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Serialize, PreserveReferencesHandling = PreserveReferencesHandling.All };
                var serialized = JsonConvert.SerializeObject(source, s);
                return JsonConvert.DeserializeObject<T>(serialized)!;
            }
            else
                return null;
        }

        public static bool In<T>(this T value, params T[] values)
        {
            if (value == null || values == null || values.Length == 0) return false;

            return values.ToList().Any(x => x!.Equals(value));
        }

        public static bool IsNumeric(this object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression.GetType().IsNumericType())
                return true;

            try
            {
                if (Expression is string)
                {
                    // Cast para string
                    string expression = Expression.ToSafeString();

                    // Se estiver vazio, não é numérico
                    if (expression.IsNullOrWhiteSpace()) return false;

                    //Simple cast to Type or null, thanks to AS operator
                    if (double.TryParse(expression, out double aut))
                        return true;
                    else
                        return expression == Regex.Match(expression, "[0-9.,]+").Value;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Utilizado quando você precisa fazer cast de uma class filha para a classe pai
        /// Ou quando você precisa trabalhar com classes muito parecidas, porém com nomes diferentes
        /// Podendo colocar os attributos de Json para fazer a conversão correta
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Y? CreateInheritedInstance<T, Y>(this T source) where Y : T, new() where T : new()
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<Y>(serialized);
        }

        public static bool HasProperty(this object val, string propertyName)
        {
            Type type = val.GetType();
            return type.GetProperties().Any(p => p.Name.Equals(propertyName));
        }

        /// <summary>
        ///     Verifica se qualquer propriedade pública do objeto possui valor
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeEnums">
        ///     Verifica se deve verificar os valores que são enumeradores, já que enums caso não sejam Nullable<>, sempre cntém
        ///     valores.
        /// </param>
        /// <param name="onlyValidStrings">Permite especificar se apenas as strings não vaziaas e nulas serão consideradas "valor"</param>
        /// <param name="ignoreProperty">
        ///     Propriedades a serem ignoradas pela conferência. Atenção: Nomes simples, pode haver
        ///     repetição na hierarquia do objeto que será considerado o mesmo.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        ///     Permite avaliar se há um nullo lógico na informação contida no objeto
        /// </remarks>
        public static bool AnyPropertyHasValue(this object obj, bool includeEnums, bool onlyValidStrings = true, params string[] ignoreProperty)
        {
            bool hasvalue = false;
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
                if (ignoreProperty == null || !ignoreProperty.Contains(pi.Name))
                {
                    if (pi.PropertyType.GetGenericArguments().Any() && pi.PropertyType.GetGenericArguments().First().IsEnum)
                    {
                        if (includeEnums) hasvalue = pi.GetValue(obj) != null;
                    }
                    else if (pi.PropertyType.IsEnum)
                    {
                        if (includeEnums) hasvalue = pi.GetValue(obj) != null;
                    }
                    else if (pi.PropertyType.Name.ToLower() == "string")
                    {
                        hasvalue = !string.IsNullOrWhiteSpace((string)pi.GetValue(obj)!);
                    }
                    else if (pi.PropertyType.IsClass)
                    {
                        if (pi.GetValue(obj) != null) hasvalue = AnyPropertyHasValue(pi.GetValue(obj)!, includeEnums, onlyValidStrings, ignoreProperty);
                    }
                    else
                    {
                        hasvalue = pi.GetValue(obj) != null;
                    }

                    if (hasvalue) break;
                }

            return hasvalue;
        }

        public static List<PropertyInfo> Properties(this object objeto, Type? tipoAtributo = null, bool excluirTipoEspecificado = false)
        {
            List<PropertyInfo> props = [.. objeto.GetType().GetProperties()];

            if (tipoAtributo != null && !excluirTipoEspecificado) props = props.Where(prop => prop.GetCustomAttribute(tipoAtributo!) != null).ToList();

            if (tipoAtributo != null && excluirTipoEspecificado) props = props.Where(prop => prop.GetCustomAttribute(tipoAtributo!) == null).ToList();

            return props;
        }
    }
}
