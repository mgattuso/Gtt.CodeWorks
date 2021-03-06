using System.Threading.Tasks;

namespace Gtt.CodeWorks.Security
{
    public interface IEncryptionService
    {
        EncryptedData Encrypt(EncryptionStrength strength, string str);
        string Decrypt(string str);
        string Hash(EncryptionStrength strength, string str, bool caseInsensitive);
        int RandomNumber(int startInclusive, int endExclusive);
        string GenerateRandomKey(EncryptionStrength strength, bool alphaNumericOnly = true);

    }

    public enum EncryptionStrength
    {
        Sha256,
        Sha512
    }
}