namespace KriziciServer.Common.Responses
{
    public class ImageResponse : IResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public byte[] Response { get; set; }
    }
}