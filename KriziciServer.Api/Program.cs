using KriziciServer.Common.Services;

namespace KriziciServer.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var serviceHost = ServiceHost.Create<Startup>(args)
                .UseRabbitMq();

            serviceHost.Build().Run();
        }
    }
}