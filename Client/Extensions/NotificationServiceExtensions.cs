using AnjUx.Client.Services;

namespace AnjUx.Client.Extensions
{
    public static class NotificationServiceExtensions
    {
        public static void NotifyException(this CoreNotificationService notificationService, Exception ex, Action<Notificacao>? click = null, object? payload = null, Action<Notificacao>? close = null)
        {
            Notificacao msg = new()
            {
                Tipo = NotificacaoTipo.Erro,
                Resumo = ex.Message,
                Detalhes = ex.ToString(),
                Duracao = -1,
                Click = click,
                CloseOnClick = false,
                Payload = payload,
                Close = close,
                Style = "width: 80vw;",
            };

            notificationService.Notify(msg);
        }

        public static void NotifyFail(this CoreNotificationService notificationService, string message, Action<Notificacao>? click = null, object? payload = null, Action<Notificacao>? close = null)
        {
            Notificacao msg = new()
            {
                Tipo = NotificacaoTipo.Erro,
                Resumo = "Falha",
                Detalhes = message,
                Duracao = -1,
                Click = click,
                CloseOnClick = false,
                Payload = payload,
                Close = close,
                Style = "width: 80vw;",
            };

            notificationService.Notify(msg);
        }
    }
}
