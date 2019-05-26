namespace KriziciServer.Common.Responses
{
    public class LoginResponse : IResponse
    {
        public bool Success { get; set; }
        public string Response { get; set; }
        public long Expires { get; set; }
    }
}