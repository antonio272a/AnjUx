using AnjUx.Client.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace AnjUx.Client.Stores
{
    public class CoreNavBarItem
    {
        public CoreNavBarItem(Icon icone, Type componente, string? tooltip = null)
        {
            Icone = icone;
            Componente = componente;
            Tooltip = tooltip;
        }

        public CoreNavBarItem(Icon icone, string view, string? contexto = null, string? area = null, string? tooltip = null)
        {
            Icone = icone;
            View = view;
            Contexto = contexto;
            Area = area;
            Tooltip = tooltip;
        }

        public CoreNavBarItem(Icon icone, Action acao, string? tooltip = null)
        {
            Icone = icone;
            Acao = acao;
            Tooltip = tooltip;
        }

        public Icon Icone { get; private set; }
        public string? Tooltip { get; private set; }
        public Type? Componente { get; private set; }
        public string? View { get; private set; }
        public string? Contexto { get; private set; }
        public string? Area { get; private set; }
        public Action? Acao { get; private set; }
    }

    public class CoreNavBarStore(IJSRuntime jsRuntime, IServiceScopeFactory scopeFactory) : CoreBaseStore(jsRuntime, scopeFactory)
    {
        private List<CoreNavBarItem> ItensAnteriores { get; set; } = [];

        private List<CoreNavBarItem> ItensPosteriores { get; set; } = [];

        private List<CoreNavBarItem> Itens { get; set; } = [];

        private string? titulo;

        public string? Titulo
        {
            get => titulo;
            set
            {
                titulo = value;
                NotifyStateChange();
            }
        }

        public IReadOnlyList<CoreNavBarItem> ItensNavBar => [.. ItensAnteriores, .. Itens, .. ItensPosteriores];

        public void Initialize(List<CoreNavBarItem> itensAnteriores, List<CoreNavBarItem> itensPosteriores)
        {
            if (Inicializado) return;

            ItensAnteriores = itensAnteriores;
            ItensPosteriores = itensPosteriores;

            Inicializado = true;
        }

        public void SetItens(params CoreNavBarItem[] itens)
        {
            Itens.Clear();
            Itens.AddRange(itens);
            NotifyStateChange();
        }

        public void Clear()
        {
            Itens.Clear();
            titulo = null;
            NotifyStateChange();
        }
    }
}
