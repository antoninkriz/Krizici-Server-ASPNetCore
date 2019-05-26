using System.Threading.Tasks;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Common.Services;

namespace KriziciServer.Services.Data
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceHost = ServiceHost.Create<Startup>(args)
                .UseRabbitMq();

            await serviceHost.SubscribeToRcp<ImageRequest, ImageResponse>();
            await serviceHost.SubscribeToRcp<DataRequest, DataResponse>();

            serviceHost.Build().Run();
        }
    }
}