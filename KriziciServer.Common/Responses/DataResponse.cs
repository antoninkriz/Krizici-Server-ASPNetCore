namespace KriziciServer.Common.Responses
{
    public class DataResponse : IResponse
    {
        public bool Success { get; set; }
        public string Response { get; set; }
    }
}