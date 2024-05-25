using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnjUx.Shared.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool IsNotNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static int? ToSafeInt(this string? value)
        {
            if (value.IsNullOrWhiteSpace()) return null;

            if (int.TryParse(value, out int result))
                return result;
            else
                return null;
        }

        public static long? ToSafeLong(this string? value)
        {
            if (value.IsNullOrWhiteSpace()) return null;

            if (long.TryParse(value, out long result))
                return result;
            else
                return null;
        }

        public static double? ToSafeDouble(this string? value)
        {
            if (value.IsNullOrWhiteSpace()) return null;

            if (double.TryParse(value, out double result))
                return result;
            else
                return null;
        }

        public static decimal? ToSafeDecimal(this string? value)
        {
            if (value.IsNullOrWhiteSpace()) return null;

            if (decimal.TryParse(value, out decimal result))
                return result;
            else
                return null;
        }

        public static string ToSafeString(this object? value)
        {
            if (value == null)
                return "";
            else
                return value.ToString()!;
        }

        public static string GetDigits(this string value)
        {
            return Regex.Replace(value, "[^0-9]", "");
        }

        public static string GetChars(this string value)
        {
            return Regex.Replace(value, "[^a-zA-Z]", "");
        }

        public static int ToInt(this string value)
        {
            return int.Parse(value);
        }

        public static long ToLong(this string value)
        {
            return long.Parse(value);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : $"{value[..(maxLength - 3)]}...";
        }

        public static string RemoveLeftZeros(this string valor)
        {
            int x = 0;

            for (int i = 0; i <= valor.Length; i++)
                if (valor.Substring(i, 1) == "0")
                    x += 1;
                else
                    break;

            return valor.Substring(x, valor.Length - x);
        }

        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        ///     Remove espaços em branco "excedentes" em um dado texto mantendo a separação entre palavras e termos com um único
        ///     espaço.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Exemplo de entrada:O Capim    é verde   quando       novo.
        ///     Saída:O capim é verde quando novo.
        /// </remarks>
        public static string RemoveOrphanSpaces(this string str)
        {
            //Segurança para que a regex não falhe
            if (!string.IsNullOrWhiteSpace(str))
                return Regex.Replace(str, @"\s+", " ");
            else
                return str;
        }

        public static string Captalize(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return char.ToUpper(s[0]) + s[1..].ToLower();
        }

        public static string ConcatSafe(this string value, string concatString, string separator = " ")
        {
            if (value.IsNullOrWhiteSpace()) return concatString;

            return $"{value}{separator}{concatString}";
        }

        public static string ToOneLine(this string? valor)
        {
            if (valor.IsNullOrWhiteSpace())
                return string.Empty;

            // Removemos espaços do início e final
            var novoValor = valor.Trim();

            // Substituímos tabs por espaços
            novoValor = novoValor.Replace("\\t", " ");

            // Removemos pulos de linha
            novoValor = novoValor.ReplaceLineEndings(" ");

            // Trocamos múltiplos espaços por um único
            novoValor = Regex.Replace(novoValor, "\\ +", " ");

            return novoValor;
        }

        public static string GetUpperCaseChars(this string input)
        {
            return new string(input.Where(char.IsUpper).ToArray());
        }

        /// <summary>
        /// EM uma lista de string, busca todas as string que contém todos os filtros indicados.
        /// </summary>
        /// <param name="value">Lista de String que será pesquisada.</param>
        /// <param name="filters">Strings que serão comparadas na lista de string.</param>
        /// <returns>Retorna uma lista de string que contém todos os filtros passados.</returns>
        public static IEnumerable<string> ContainsValues(this IEnumerable<string> value, params string[] filters)
        {
            return value.Where(x =>
            {
                foreach (var filter in filters)
                {
                    if (!x.Contains(filter)) return false;
                }

                return true;
            });
        }

    }
}
