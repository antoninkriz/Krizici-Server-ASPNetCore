namespace KriziciServer.Common.Auth
{
    public interface IJwtHandler
    {
        JsonWebToken Create<T>(T userId);
    }
}