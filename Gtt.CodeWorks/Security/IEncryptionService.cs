using System.Threading.Tasks;

namespace Gtt.CodeWorks.Security
{
    public interface IEncryptionService
    {
        Task<EncryptedData> Encrypt(EncryptionStrength strength, string str);
        Task<string> Decrypt(string str);
        Task<string> Hash(EncryptionStrength strength, string str, bool caseInsensitive);
        int RandomNumber(int startInclusive, int endExclusive);
        Task<string> GenerateRandomKey(EncryptionStrength strength, bool alphaNumericOnly = true);
    }

    public enum EncryptionStrength
    {
        Sha256,
        Sha512
    }
}