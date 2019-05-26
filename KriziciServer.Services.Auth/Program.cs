using System.Threading.Tasks;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Common.Services;

namespace KriziciServer.Services.Auth
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceHost = ServiceHost.Create<Startup>(args)
                .UseRabbitMq();

            await serviceHost.SubscribeToRcp<LoginRequest, LoginResponse>();

            serviceHost.Build().Run();
        }
    }
}