using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Client.Services
{
    public enum NotificacaoTipo: int
    {
        Erro = 0,
        Info = 1,
        Sucesso = 2,
        Aviso = 3
    }

    public class Notificacao()
    {
        public NotificacaoTipo? Tipo { get; set; }
        public string? Resumo { get; set; }
        public string? Detalhes { get; set; }
        public double? Duracao { get; set; }
        public Action<Notificacao>? Click { get; set; }
        public bool? CloseOnClick { get; set; }
        public object? Payload { get; set; }
        public Action<Notificacao>? Close { get; set; }
        public string? Style { get; set; }
    }

    public class CoreNotificationService(NotificationService notificationService) : IServiceBase
    {
        private readonly NotificationService notificationService = notificationService;

        private static NotificationMessage Transformar(Notificacao notificacao)
        {
            NotificationMessage msg = new()
            {
                Severity = (NotificationSeverity)notificacao.Tipo!,
                Summary = notificacao.Resumo,
                Detail = notificacao.Detalhes,
                Duration = notificacao.Duracao,
                CloseOnClick = notificacao.CloseOnClick.GetValueOrDefault(),
                Payload = notificacao.Payload,
                Style = notificacao.Style,
            };

            if (notificacao.Click != null)
                msg.Click = (msg) => notificacao.Click.Invoke(notificacao);

            if (notificacao.Close != null)
                msg.Close = (msg) => notificacao.Close.Invoke(notificacao);

            return msg;
        }

        public void Notify(Notificacao notificacao) => notificationService.Notify(Transformar(notificacao));

        public void Notify(NotificacaoTipo tipo, string resumo = "", string detalhes = "", double duracao = 3000, Action<Notificacao>? click = null, bool closeOnClick = false, object? payload = null, Action<Notificacao>? close = null, string? style = null)
        {
            Notificacao notificacao = new()
            {
                Tipo = tipo,
                Resumo = resumo,
                Detalhes = detalhes,
                Duracao = duracao,
                Click = click,
                CloseOnClick = closeOnClick,
                Payload = payload,
                Close = close,
                Style = style,
            };

            notificationService.Notify(Transformar(notificacao));
        }

    }
}
