using System.Threading.Tasks;

namespace KriziciServer.Common.Requests
{
    public interface IRequestHandler<in TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}