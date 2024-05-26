﻿using AnjUx.Shared.Models.Data;

namespace AnjUx.Client.Services
{
    public class MunicipioService(HttpClient http, CoreNotificationService notificationService, LoadingService loadingService) : BaseService<Municipio>(http, notificationService, loadingService)
    {
        protected override string BaseUrl => "api/municipio";

        public async Task<bool> AtualizarMunicipios()
            => await MakeRequestAsync<bool>(HttpMethod.Get, "AtualizarMunicipios");
    }
}