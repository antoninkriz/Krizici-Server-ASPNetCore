using System;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KriziciServer.Common.GoogleDrive
{
    public static class Extensions
    {
        public static void AddGoogleDrive(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IClientService>(serviceProvider => GoogleClientServiceFactory(configuration));
        }

        private static DriveService GoogleClientServiceFactory(IConfiguration configuration)
        {
            var conf = configuration.GetSection("googleapi");

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(conf.GetValue<string>("client_email"))
                {
                    ProjectId = conf.GetValue<string>("project_id"),
                    Scopes = new[]
                    {
                        DriveService.Scope.DriveReadonly
                    }
                }.FromPrivateKey(conf.GetValue<string>("private_key")));
            
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ""
            });
        }
    }
}