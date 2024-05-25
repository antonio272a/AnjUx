using AnjUx.Shared.Extensions;
using System.Text;

namespace AnjUx.ORM.Classes
{
    public enum FiltroTipo : int
    {
        And = 0,
        Or = 1
    }
    
    public enum OperadorTipo : int
    {
        Igual = 0,
        Diferente = 1,
        Em = 2,
        NaoEm = 3,
        Maior = 4,
        MaiorIgual = 5,
        Menor = 6,
        MenorIgual = 7,
        Entre = 8,
        Like = 9,
        LikePossuindo = 10,
        LikeComecandoCom = 11,
        LikeTerminandoCom = 12,
        LikePossuindoSemOrdem = 13,
    }

    public class Filtro
    {
        public FiltroTipo? Tipo { get; set; }
        public OperadorTipo? Operador { get; set; }
        public string? Campo { get; set; }
        public string? CampoCompleto { get; set; }
        public Join? Join { get; set; }
        public string? Valor { get; set; }
        public string? ValorDe { get; set; }
        public string? ValorPara { get; set; }
        public string? PlainFilter { get; set; }
        

        public Filtro(FiltroTipo tipo, OperadorTipo operador, string campoCompleto, object? valor) 
        {
            Tipo = tipo;
            CampoCompleto = campoCompleto;
            Operador = operador;
            if (valor != null) Valor = QueryBuilder.ValorPrimitivoParaString(valor);
        }

        public Filtro(FiltroTipo tipo, OperadorTipo operador, Join join, string campo, object? valor)
        {
            Tipo = tipo;
            Campo = campo;
            Join = join;
            Operador = operador;
            if(valor != null) Valor = QueryBuilder.ValorPrimitivoParaString(valor);
        }

        public Filtro(FiltroTipo tipo, OperadorTipo operador, QueryModel query, string campo, object? valor)
        {
            if (query.Alias.IsNullOrWhiteSpace())
                throw new Exception("A Query não possui um Alias para o Select principal.");

            Tipo = tipo;
            CampoCompleto = $"{query.Alias}.{campo}";
            Operador = operador;
            
            if (valor != null) 
                Valor = QueryBuilder.ValorPrimitivoParaString(valor);
        }

        public Filtro(FiltroTipo tipo, Join join, string campo, object valorDe, object valorPara)
        {
            Tipo = tipo;
            Campo = campo;
            Join = join;
            Operador = OperadorTipo.Entre;
            ValorDe = QueryBuilder.ValorPrimitivoParaString(valorDe);
            ValorPara = QueryBuilder.ValorPrimitivoParaString(valorPara);
        }

        public Filtro(FiltroTipo tipo, string campoCompleto, object valorDe, object valorPara)
        {
            Tipo = tipo;
            CampoCompleto = campoCompleto;
            Operador = OperadorTipo.Entre;
            ValorDe = QueryBuilder.ValorPrimitivoParaString(valorDe);
            ValorPara = QueryBuilder.ValorPrimitivoParaString(valorPara);
        }

        public Filtro(string plainFilter)
        {
            PlainFilter = plainFilter;
        }

        public override string ToString()
        {
            if (!PlainFilter.IsNullOrWhiteSpace()) 
                return PlainFilter!;

            if (Operador == OperadorTipo.Entre && (ValorDe != null || ValorPara == null))
                throw new Exception($"Você deve informar os valores intermediários para utilizar o operador {OperadorTipo.Entre.GetDescriptionEnum()}");

            StringBuilder sb = new();

            if (Join != null) 
                CampoCompleto = $"{Join!.Alias}.{Campo}";

            switch (Tipo)
            {
                case FiltroTipo.And:
                    sb.Append($"AND {CampoCompleto}");
                    break;

                case FiltroTipo.Or:
                    sb.Append($"OR {CampoCompleto}");
                    break;
            }

            switch (Operador)
            {
                case OperadorTipo.Igual:
                    if (Valor == null) 
                        sb.Append(" IS NULL");
                    else 
                        sb.Append($" = {Valor}");
                    break;
                case OperadorTipo.Diferente:
                    if (Valor == null)
                        sb.Append(" IS NOT NULL");
                    else
                        sb.Append($" <> {Valor}");
                    break;
                case OperadorTipo.Em:
                    sb.Append($" IN ({Valor})");
                    break;
                case OperadorTipo.NaoEm:
                    sb.Append($" NOT IN ({Valor})");
                    break;
                case OperadorTipo.Maior:
                    sb.Append($" > {Valor}");
                    break;
                case OperadorTipo.MaiorIgual:
                    sb.Append($" >= {Valor}");
                    break;
                case OperadorTipo.Menor:
                    sb.Append($" < {Valor}");
                    break;
                case OperadorTipo.MenorIgual:
                    sb.Append($" <= {Valor}");
                    break;
                case OperadorTipo.Entre:
                    sb.Append($" BETWEEN {ValorDe} AND {ValorPara}");
                    break;
                case OperadorTipo.Like:
                    sb.Append($" LIKE {Valor}");
                    break;
                // A partir desse case, removemos as aspas simples para poder adicionar os % no inicio e/ou no fim
                case OperadorTipo.LikePossuindo:
                    sb.Append($" LIKE \'%{Valor!.Replace("'", "")}%\'");
                    break;
                case OperadorTipo.LikeComecandoCom:
                    sb.Append($" LIKE \'{Valor!.Replace("'", "")}%\'");
                    break;
                case OperadorTipo.LikeTerminandoCom:
                    sb.Append($" LIKE \'%{Valor!.Replace("'", "")}\'");
                    break;
                case OperadorTipo.LikePossuindoSemOrdem:
                    sb.Append($" LIKE \'%{Valor!.Replace("'", "").Split(' ').ToCommaString(separator: "%")}%\'");
                    break;
                default:
                    throw new NotImplementedException("Enumerador de Operação não implementado");
            }

            return sb.ToString();
        }
    }
}
