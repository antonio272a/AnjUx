using AnjUx.Client.Extensions;
using AnjUx.Client.Shared;
using AnjUx.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Reflection;

namespace AnjUx.Client.Services
{
    public class CoreDialogOptions : DialogOptions
    {
    }

    public class CoreAlertOptions : AlertOptions
    {
    }

    public class CoreConfirmOptions : ConfirmOptions
    {
    }

    public class CoreDialogService : IServiceBase
    {
        private readonly DialogService _dialogService;
        private readonly NavigationManager _navigationManager;

        private readonly List<Action<bool>> actions = [];

        public CoreDialogService(DialogService dialogService, NavigationManager navigationManager)
        {
            _dialogService = dialogService;
            _navigationManager = navigationManager;

            _dialogService.OnClose += OnClose;
        }

        private readonly CoreDialogOptions defaultOptions = new()
        {
            Draggable = true,
            Resizable = true,
            CloseDialogOnOverlayClick = true,
            CloseDialogOnEsc = true
        };

        private void OnClose(dynamic result)
        {
            bool atualizar = result != null && result != false; 

            foreach (Action<bool> action in actions)
                action?.Invoke(atualizar);

            actions.Clear();
        }

        public void Open(string title, string view, string? context = null, string? area = null, object? parameters = null, CoreDialogOptions? options = null, Action<bool>? onClose = null)
        {
            string rota = _navigationManager.GetRoute(view, context, area);

            if(!rota.StartsWith('/'))
                rota = $"/{rota}";

            Type componente = (CoreRoutes.Instance.Componentes.TryGetValue(rota.ToLower(), out Type? value) ? value : null) ?? throw new ArgumentException("Componente não encontrado");

            Open(componente, title, parameters, options, onClose);
        }

        public void Open(Type component, string title, object? parameters = null, CoreDialogOptions? options = null, Action<bool>? onClose = null)
        {
            if (!component.IsSubclassOf(typeof(ComponentBase)))
                throw new ArgumentException("O tipo informado não é um componente!");

            Type[] methodParameterTypes = [typeof(string), typeof(Dictionary<string, object>), typeof(DialogOptions)];

            MethodInfo openMethod = typeof(DialogService).GetMethod("Open", BindingFlags.Public | BindingFlags.Instance, null, methodParameterTypes, null)!;
            MethodInfo genericOpenMethod = openMethod.MakeGenericMethod(component);

            genericOpenMethod.Invoke(_dialogService, [title, parameters?.ToDictionary(), options ?? defaultOptions]);

            if (onClose != null) actions.Add(onClose);
        }

        public void Open<T>(string title, object? parameters = null, CoreDialogOptions? options = null) where T : ComponentBase => _dialogService.Open<T>(title, parameters?.ToDictionary(), options ?? defaultOptions);

        public void Close(bool resultado = false) => _dialogService.Close(resultado);

        public async Task<bool?> Confirm(string message, string title, CoreConfirmOptions? options = null) => await _dialogService.Confirm(message, title, options);

        public async Task<bool?> Alert(string message, string title, CoreAlertOptions? options = null) => await _dialogService.Alert(message, title, options);
    }
}
