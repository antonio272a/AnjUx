using AnjUx.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnjUx.Client.Extensions
{
    public static class NavigationManagerExtensions
    {
        public static void IrPara(this NavigationManager navigationManager, string view, string? contexto = null, string? area = null, object? routeValues = null, bool forceLoad = false)
        {
            string rotaAtual = navigationManager.Uri.Replace(navigationManager.BaseUri, "");

            string url = BuildUrl(rotaAtual, view, area, contexto);

            string queryString = SerializeRouteValuesToQueryString(routeValues);

            if (!string.IsNullOrWhiteSpace(queryString))
                url += "?" + queryString;

            url = url.Replace("/*", "");

            navigationManager.NavigateTo(url, forceLoad);
        }

        public static void IrPara(this NavigationManager navigationManager, Type page, object? routeValues = null, bool forceLoad = false)
        {
            string rota = GetRoute(page);

            string queryString = SerializeRouteValuesToQueryString(routeValues);

            if (!string.IsNullOrWhiteSpace(queryString))
                rota += "?" + queryString;

            rota = rota.Replace("/*", "");

            navigationManager.NavigateTo(rota, forceLoad);
        }

        private static string GetRoute(Type page) => CoreRoutes.Instance.GetRoute(page);

        public static string GetRoute(this NavigationManager navigationManager, string view, string? contexto = null, string? area = null)
        {
            string rotaAtual = navigationManager.Uri.Replace(navigationManager.BaseUri, "");

            return BuildUrl(rotaAtual, view, area, contexto);
        }

        private static string SerializeRouteValuesToQueryString(object? routeValues)
        {
            if (routeValues == null)
                return string.Empty;

            var properties = routeValues.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                               .Where(p => p.GetValue(routeValues) != null)
                               .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(routeValues)!.ToString()!)}");

            return string.Join("&", properties);
        }

        private static string BuildUrl(string rotaAtual, string view, string? area, string? contexto)
        {
            (string areaAtual, string contextoAtual) = GetAreaContexto(rotaAtual);

            contexto ??= contextoAtual;
            area ??= areaAtual;

            if (view.Equals("index", StringComparison.CurrentCultureIgnoreCase))
                view = string.Empty;

            StringBuilder urlBuilder = new();

            if (!string.IsNullOrWhiteSpace(area))
                urlBuilder.Append($"{area}/");

            if (!string.IsNullOrWhiteSpace(contexto))
                urlBuilder.Append($"{contexto}");

            if (!string.IsNullOrWhiteSpace(view))
                urlBuilder.Append($"/{view}");

            string url = urlBuilder.ToString();

            if (url.EndsWith('/') && url != "/")
                url = url[..^1];

            return url;
        }

        private static (string area, string contexto) GetAreaContexto(string rota)
        {
            List<string> partes = [.. rota.Split('/')];

            string areaAtual;
            string contextoAtual;

            if (partes.Count <= 1)
            {
                areaAtual = string.Empty;
                contextoAtual = string.Empty;
            }
            else
            {
                string possivelContexto = partes[0];
                if (CoreRoutes.Instance.Contextos.Contains(possivelContexto.ToLower()))
                {
                    areaAtual = "";
                    contextoAtual = possivelContexto;
                }
                else
                {
                    areaAtual = possivelContexto;
                    contextoAtual = CoreRoutes.Instance.Contextos.Contains(partes[1].ToLower()) ? partes[1] : "";
                }
            }

            return (areaAtual, contextoAtual);
        }

        public static bool TryGetQueryParam<T>(this NavigationManager navManager, string key, out T? value)
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
            {
                string? stringValue = valueFromQueryString.FirstOrDefault();

                if (stringValue != null)
                {
                    try
                    {
                        value = (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(stringValue);
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            value = default;
            return false;
        }

        public static T? GetQueryParam<T>(this NavigationManager navManager, string key)
        {
            if (navManager.TryGetQueryParam(key, out T? value))
                return value;

            throw new ArgumentException($"Query string não encontrada ou tipo inválido para a chave '{key}'");
        }
    }
}
