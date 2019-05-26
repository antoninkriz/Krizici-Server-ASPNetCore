using System.Threading.Tasks;

namespace KriziciServer.Services.Data.Services
{
    public interface IDataService
    {
        Task<string> GetDataAsync(Types type);
        Task<byte[]> GetContentAsync(string type, int id);
        Task UpdateDataAsync();
    }

    public enum Types
    {
        Contacts,
        Data
    }
}