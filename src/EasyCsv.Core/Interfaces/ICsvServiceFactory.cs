using System.Threading.Tasks;

namespace EasyCsv.Core
{
    public interface ICsvServiceFactory : IEasyService
    {
        ICsvService? CreateFromBytes(byte[] fileContent, bool normalizeFields = false);
        Task<ICsvService?> CreateFromBytesInBackground(byte[] fileContent, bool normalizeFields = false);
        ICsvService? CreateFromString(string fileContent, bool normalizeFields = false);
        Task<ICsvService?> CreateFromStringInBackground(string fileContent, bool normalizeFields = false);
    }

}
