using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using KriziciServer.Common.Auth;
using KriziciServer.Common.Exceptions;
using JsonWebToken = KriziciServer.Common.Auth.JsonWebToken;

namespace KriziciServer.Services.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtHandler _jwtHandler;

        public AuthService(IJwtHandler jwtHandler)
        {
            _jwtHandler = jwtHandler;
        }
        
        public async Task<JsonWebToken> LoginAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload validPayload;

            try
            {
                validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            }
            catch (Exception)
            {
                throw new AuthException("invalid_id_token_1");
            }
            
            if (validPayload == null)
                throw new AuthException("invalid_id_token_2");

            if (validPayload.HostedDomain != "skolakrizik.cz")
                throw new AuthException("email_not_in_domain");

            return _jwtHandler.Create(validPayload.Email);
        }
    }
}