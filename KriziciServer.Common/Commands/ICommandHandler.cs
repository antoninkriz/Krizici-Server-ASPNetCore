using System.Threading.Tasks;

namespace KriziciServer.Common.Commands
{
    public interface ICommandHandler<in T>
    {
        Task HandleAsync(T command);
    }
}