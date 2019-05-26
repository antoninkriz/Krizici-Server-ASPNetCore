using System.Reflection;
using System.Threading.Tasks;
using KriziciServer.Common.Commands;
using KriziciServer.Common.Events;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RawRabbit;
using RawRabbit.Enrichers.GlobalExecutionId;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;
using BusClient = RawRabbit.Instantiation.Disposable.BusClient;

namespace KriziciServer.Common.RabbitMq
{
    public static class Extensions
    {
        public static async Task WithCommandHandlerAsync<TCommand>(this IBusClient bus, ICommandHandler<TCommand> handler)
            where TCommand : ICommand
            => await bus.SubscribeAsync<TCommand>(handler.HandleAsync,
                ctx => ctx.UseSubscribeConfiguration(cfg => cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TCommand>()))));

        public static async Task WithEventHandlerAsync<TEvent>(this IBusClient bus, IEventHandler<TEvent> handler)
            where TEvent : IEvent
            => await bus.SubscribeAsync<TEvent>(handler.HandleAsync,
                ctx => ctx.UseSubscribeConfiguration(cfg => cfg.FromDeclaredQueue(q => q.WithName(GetQueueName<TEvent>()))));

        public static async Task WithRequestResponseAsync<TRequest, TResponse>(this IBusClient bus, IRequestHandler<TRequest, TResponse> requestHandler)
            where TRequest : IRequest where TResponse : IResponse
            => await bus.RespondAsync<TRequest, TResponse>(request => requestHandler.HandleAsync(request));

        private static string GetQueueName<T>()
        {
            return $"{Assembly.GetEntryAssembly().GetName()}/{typeof(T).Name}";
        }

        private static BusClient RabbitMqClientFactory(IConfiguration configuration)
        {
            var options = new RabbitMqOptions();
            var section = configuration.GetSection("rabbitmq");
            section.Bind(options);

            return RawRabbitFactory.CreateSingleton(new RawRabbitOptions
            {
                ClientConfiguration = options,
                Plugins = p => p
                    .UseGlobalExecutionId()
                    .UseMessageContext<MessageContext>()
            });
        }

        public static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBusClient>(_ => RabbitMqClientFactory(configuration));
        }
    }
}