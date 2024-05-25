using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace AnjUx.Client.Stores
{
    public abstract class CoreBaseStore(IJSRuntime jsRuntime, IServiceScopeFactory scopeFactory)
    {
        protected readonly IJSRuntime _jsRuntime = jsRuntime;
        protected readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        protected readonly List<Action> Listeners = [];

        protected bool Inicializado { get; set; } = false;

        public void AddListener(Action listener)
        {
            Listeners.Add(listener);
        }

        public void RemoveListener(Action listener)
        {
            Listeners.Remove(listener);
        }

        protected void NotifyStateChange()
        {
            foreach (var listener in Listeners)
                listener.Invoke();
        }

        protected async Task<string?> GetLocalStorage(string key)
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        protected async Task SetLocalStorage(string key, string value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
    }
}
