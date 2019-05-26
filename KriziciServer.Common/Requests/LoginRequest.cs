namespace KriziciServer.Common.Requests
{
    public class LoginRequest : IRequest
    {
        public string IdToken { get; set; }
    }
}