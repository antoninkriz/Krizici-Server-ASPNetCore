using System.Threading.Tasks;
using KriziciServer.Common.Exceptions;
using KriziciServer.Common.Requests;
using KriziciServer.Common.Responses;
using KriziciServer.Services.Auth.Services;
using Microsoft.Extensions.Logging;

namespace KriziciServer.Services.Auth.Handlers
{
    public class RequestLoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly IAuthService _authService;
        private readonly ILogger _logger;

        public RequestLoginHandler(IAuthService authService, ILogger<RequestLoginHandler> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<LoginResponse> HandleAsync(LoginRequest request)
        {
            var shortToken = request.IdToken.Substring(0, 8);
            _logger.LogInformation($"Trying to login token: '{shortToken}...'.");

            var response = new LoginResponse();

            try
            {
                var token = await _authService.LoginAsync(request.IdToken);
                response.Success = true;
                response.Response = token.Token;
                response.Expires = token.Expires;
            }
            catch (AuthException e)
            {
                response.Success = false;
                response.Response = e.Code;
            }

            _logger.LogInformation($"Login for token '{shortToken}...' successful");

            return response;
        }
    }
}