using AnjUx.Shared.Models.Data;
using System.ComponentModel.DataAnnotations;

namespace AnjUx.Shared.Models.Response
{
    public class ComparadorModel
    {
        public int? AnoInicial { get; set; }
        public int? AnoFinal { get; set; }
        public int? MesInicial { get; set; }
        public int? MesFinal { get; set; }
        public List<ComparadorMunicipio> Municipios { get; set; } = [];
    }

    public class ComparadorMunicipio
    {
        public Municipio? Municipio { get; set; }
        public List<MunicipioDado> DadosIniciais { get; set; } = [];
        public List<MunicipioDado> DadosFinais { get; set; } = [];

        public decimal ReceitaInicial => Math.Round(DadosIniciais.FirstOrDefault(d => d.TipoDado == TipoDado.Receita)?.Valor ?? 0, 2);
        public decimal ReceitaFinal => Math.Round(DadosFinais.FirstOrDefault(d => d.TipoDado == TipoDado.Receita)?.Valor ?? 0, 2);
        public decimal PopulacaoInicial => DadosIniciais.FirstOrDefault(d => d.TipoDado == TipoDado.Populacao)?.Valor ?? 0;
        public decimal PopulacaoFinal => DadosFinais.FirstOrDefault(d => d.TipoDado == TipoDado.Populacao)?.Valor ?? 0;

        public decimal ReceitaPerCaptaInicial => PopulacaoInicial != 0 ? Math.Round(ReceitaInicial / PopulacaoInicial, 2) : 0;
        public decimal ReceitaPerCaptaFinal => PopulacaoFinal != 0 ? Math.Round(ReceitaFinal / PopulacaoFinal, 2) : 0;

        public decimal ReceitaVariacao => Math.Round(ReceitaFinal - ReceitaInicial, 2);
        public decimal ReceitaVariacaoPercentual => ReceitaInicial != 0 ? Math.Round((ReceitaVariacao / ReceitaInicial), 4) : 0;
        public decimal ReceitaPerCaptaVariacao => Math.Round(ReceitaPerCaptaFinal - ReceitaPerCaptaInicial, 2);
        public decimal ReceitaPerCaptaVariacaoPercentual => ReceitaPerCaptaInicial != 0 ? Math.Round((ReceitaPerCaptaVariacao / ReceitaPerCaptaInicial), 4) : 0;

        #region Display

        [Display(Name = "Arrecadação Inicial")]
        public string ArrecadacaoInicialDisplay => ReceitaInicial.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Arrecadação Final")]
        public string ArrecadacaoFinalDisplay => ReceitaFinal.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Arrecadação Per Capta Inicial")]
        public string ArrecadacaoPerCaptaInicialDisplay => ReceitaPerCaptaInicial.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Arrecadação Per Capta Final")]
        public string ArrecadacaoPerCaptaFinalDisplay => ReceitaPerCaptaFinal.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Variação de Arrecadação")]
        public string ArrecadacaoVariacaoDisplay => ReceitaVariacao.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Variação de Arrecadação (%)")]
        public string ArrecadacaoVariacaoPercentualDisplay => ReceitaVariacaoPercentual.ToString("P2", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Variação de Arrecadação Per Capta")]
        public string ArrecadacaoPerCaptaVariacaoDisplay => ReceitaPerCaptaVariacao.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        [Display(Name = "Variação de Arrecadação Per Capta (%)")]
        public string ArrecacacaoPerCaptaVariacaoPercentualDisplay => ReceitaPerCaptaVariacaoPercentual.ToString("P2", new System.Globalization.CultureInfo("pt-BR"));


        #endregion
    }
}
