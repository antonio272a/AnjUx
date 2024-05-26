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

        [Display(Name = "Receita Inicial")]
        public decimal ReceitaInicial => DadosIniciais.FirstOrDefault(d => d.TipoDado == TipoDado.Receita)?.Valor ?? 0;
        [Display(Name = "Receita Final")]
        public decimal ReceitaFinal => DadosFinais.FirstOrDefault(d => d.TipoDado == TipoDado.Receita)?.Valor ?? 0;
        public decimal PopulacaoInicial => DadosIniciais.FirstOrDefault(d => d.TipoDado == TipoDado.Populacao)?.Valor ?? 0;
        public decimal PopulacaoFinal => DadosFinais.FirstOrDefault(d => d.TipoDado == TipoDado.Populacao)?.Valor ?? 0;

        [Display(Name = "Receita Per Capta Inicial")]
        public decimal ReceitaPerCaptaInicial => Math.Round(ReceitaInicial / PopulacaoInicial, 2);
        [Display(Name = "Receita Per Capta Final")]
        public decimal ReceitaPerCaptaFinal => Math.Round(ReceitaFinal / PopulacaoFinal, 2);

        [Display(Name = "Variação de Receita")]
        public decimal ReceitaVariacao => ReceitaFinal - ReceitaInicial;
        [Display(Name = "Variação de Receita (%)")]
        public decimal ReceitaVariacaoPercentual => Math.Round((ReceitaVariacao / ReceitaInicial) * 100, 2);
        [Display(Name = "Variação de Receita Per Capta")]
        public decimal ReceitaPerCaptaVariacao => ReceitaPerCaptaFinal - ReceitaPerCaptaInicial;
        [Display(Name = "Variação de Receita Per Capta (%)")]
        public decimal ReceitaPerCaptaVariacaoPercentual => Math.Round((ReceitaPerCaptaVariacao / ReceitaPerCaptaInicial) * 100, 2);

    }
}
