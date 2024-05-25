using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string ToString<T>(this IEnumerable<T> list, Expression<Func<T, string>> itemFormatterExpr, string separator = ", ")
        {
            // Validate if the list is null
            if (list == null) return string.Empty;

            // Compile the expression to get the delegate
            var itemFormatter = itemFormatterExpr.Compile();

            // Do it
            return string.Join(separator, list.Select(value => Rescue(value, itemFormatter, itemFormatterExpr)));
        }

        public static string Rescue<T>(T value, Func<T, string> itemFormatter, Expression<Func<T, string>> itemFormatterExpr)
        {
            try
            {
                return itemFormatter.Invoke(value);
            }
            catch (Exception ex)
            {
                string literalFormatter = itemFormatterExpr.ToString();
                throw new FormatException($"The formatter {literalFormatter} threw an Exception: {ex.Message}", ex);
            }
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection, Func<T, bool> predicate)
        {
            return collection == null || !collection.Any(predicate);
        }

        public static long SafeCount<T>(this IEnumerable<T>? value)
        {
            if (value == null)
                return 0;

            return value.Count();
        }

        public static string ToCommaString<T>(this IEnumerable<T> value, bool useQuotes = false, string separator = ",", bool trimmed = false)
        {
            StringBuilder sb = new();
            string spacer = trimmed ? "" : " ";

            if (useQuotes)
                foreach (T item in value)
                    sb.Append($"'{item.ToSafeString()}'{separator}{spacer}");
            else
                foreach (T item in value)
                    sb.Append($"{item.ToSafeString()}{separator}{spacer}");

            if (sb.Length > 1)
            {
                int count = trimmed ? 1 : 2;
                sb.Remove(sb.Length - count, count);
            }

            return sb.ToString();
        }

        public static Dictionary<B, List<A>> ToGroupedDictionary<A, B>(this IEnumerable<A> colecao, Func<A, B> expressaoLambda)
            where B : notnull
        {
            var dicionario = new Dictionary<B, List<A>>();
            var agrupamento = colecao.GroupBy(expressaoLambda);

            if (!agrupamento.Any())
                return dicionario;

            foreach (var grupo in agrupamento)
            {
                dicionario[grupo.Key] = grupo.ToList();
            }

            return dicionario;
        }

        public static string ToEnumCommaString<T>(this IEnumerable<T> value, bool useQuotes = false, string separator = ",", bool trimmed = false) where T : Enum
        {
            List<int> newList = value.Select(x => (int)(object)x).ToList();

            return newList.ToCommaString(useQuotes, separator, trimmed);
        }

        public static HashSet<X> ToHashSet<T, X>(this IEnumerable<T> value, Func<T, X> getter)
        {
            return value.Select(getter).ToHashSet();
        }
    }
}
