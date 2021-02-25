using System.Threading.Tasks;

namespace Gtt.CodeWorks.Security
{
    public interface IEncryptionService
    {
        Task<EncryptedData> Encrypt(string str);
        Task<string> Decrypt(string str);
        Task<string> Hash(string str, bool caseInsensitive);
        int RandomNumber(int startInclusive, int endExclusive);
    }
}