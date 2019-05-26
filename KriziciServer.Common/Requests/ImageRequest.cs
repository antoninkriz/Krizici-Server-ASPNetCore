namespace KriziciServer.Common.Requests
{
    public class ImageRequest : IRequest
    {
        public string Type { get; set; }
        public int Id { get; set; }
    }
}