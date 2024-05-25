using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class EnumExtensions
    {
        public static bool GetDisplayAutoGenerateField(this Enum value)
        {
            var attDisplay = value.GetType().GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>();

            if(attDisplay != null)
            {
                // Motivo para isso: Esse carinha retorna null caso não tiver sido indicado, e em caso de null queremos TRUE
                return !(attDisplay.GetAutoGenerateField() == false);
            }

            return true;
        }

        public static TAttribute? GetEnumAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            if (value != null)
            {
                FieldInfo? eInfo = value.GetType().GetField(value.ToString()!);
                var attribute = eInfo?.GetCustomAttribute(typeof(TAttribute));
                if (attribute != null) return (TAttribute)attribute;
            }
            return null;
        }

        public static IEnumerable<TAttribute>? GetEnumAttributes<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            if (value != null)
            {
                FieldInfo? eInfo = value.GetType().GetField(value.ToString()!);
                var attributes = eInfo?.GetCustomAttributes(typeof(TAttribute));
                if (!attributes.IsNullOrEmpty()) return attributes.Cast<TAttribute>();
            }
            return null;
        }

        public static string? GetDescriptionEnum(this Enum value)
        {
            if (value == null) return null;
            var attribute = GetEnumAttribute<DescriptionAttribute>(value); // Obtém o atributo de descrição
            return attribute?.Description ?? value.ToString();
        }

        public static List<T> GetAllValues<T>() where T : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T precisa ser  do tipo enum");

            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }

        public static T? CastEnum<T>(this object v, bool throwError = false)
        {
            // Cache
            Type eType = typeof(T);
            Type underlyingType = Enum.GetUnderlyingType(eType);

            // Pega todos os valores do enum
            Array values = Enum.GetNames(eType);
            foreach (var value in values)
            {
                T parsed = (T)Enum.Parse(eType, value.ToString()!);
                var under = Convert.ChangeType(parsed, underlyingType).ToString();
                if (under == v.ToString()) return parsed;
            }

            if (throwError) throw new InvalidCastException();
            return default;
        }
    }
}
