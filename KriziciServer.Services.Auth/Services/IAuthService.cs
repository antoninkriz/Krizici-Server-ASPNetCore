using System.Threading.Tasks;
using KriziciServer.Common.Auth;

namespace KriziciServer.Services.Auth.Services
{
    public interface IAuthService
    {
        Task<JsonWebToken> LoginAsync(string idToken);
    }
}