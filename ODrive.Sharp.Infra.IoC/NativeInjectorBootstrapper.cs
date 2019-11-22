﻿using System;
using Microsoft.Extensions.DependencyInjection;
using ODrive.Sharp.Application.Interfaces;
using ODrive.Sharp.Application.Services;
using Google.Apis.Services;
using Google.Apis.Http;
using ODrive.Sharp.Application.Presenters;
using ODrive.Sharp.Application.Providers;

namespace ODrive.Sharp.Infra.IoC
{
    public class NativeInjectorBootstrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IGoogleClientAuthProvider, GoogleClientAuthProvider>();
            services.AddScoped<IGoogleApiServiceProvider, GoogleApiServiceProvider>();

            services.AddScoped<IGoogleOAuthAppService, GoogleOAuthAppService>();
            services.AddScoped<IGoogleDriveAppService, GoogleDriveAppService>();

            services.AddScoped<IMainWindowPresenter, MainWindowPresenter>();
        }

        private static BaseClientService.Initializer GetBaseClientService(
            IConfigurableHttpClientInitializer httpClientInitializer)
        {
            return new BaseClientService.Initializer
            {
                HttpClientInitializer = httpClientInitializer,
                ApplicationName = "knuxbbs Open Drive"
            };
        }
    }
}