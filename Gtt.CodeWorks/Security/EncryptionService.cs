using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Gtt.CodeWorks.Security
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _encryptionKey;
        private readonly string _hashKey;
        private static readonly RNGCryptoServiceProvider RngCryptoServiceProvider = new RNGCryptoServiceProvider();
        private readonly byte[] _uint32Buffer = new byte[4];

        public EncryptionService(string encryptionKey, string hashKey)
        {
            _encryptionKey = encryptionKey;
            _hashKey = hashKey;
        }

        public EncryptedData Encrypt(EncryptionStrength strength, string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new EncryptedData { IsEmpty = true };
            }

            var hash = Hash(strength, str, false);

            var ekey = _encryptionKey;

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(ekey);
                var encrypted = AesEncryption.EncryptStringToBytes_Aes(str, aes.Key, aes.IV);
                var result = new byte[aes.IV.Length + encrypted.Length];
                aes.IV.CopyTo(result, 0);
                encrypted.CopyTo(result, 16);

                return new EncryptedData
                {
                    IsEmpty = false,
                    Hash = hash,
                    Encrypted = Convert.ToBase64String(result)
                };
            }
        }

        public string Decrypt(string str)
        {
            byte[] ivPlusMessage = Convert.FromBase64String(str);
            byte[] message = new byte[ivPlusMessage.Length - 16];
            byte[] iv = new byte[16];

            Array.ConstrainedCopy(ivPlusMessage, 0, iv, 0, 16);
            Array.ConstrainedCopy(ivPlusMessage, 16, message, 0, ivPlusMessage.Length - 16);

            var ekey = _encryptionKey;

            var decrypted = AesEncryption.DecryptStringFromBytes_Aes(message, Convert.FromBase64String(ekey), iv);
            return decrypted;
        }

        public string Hash(EncryptionStrength strength, string source, bool caseInsensitive)
        {
            var hkey = _hashKey;

            if (caseInsensitive)
            {
                source = source?.ToUpperInvariant();
            }

            switch (strength)
            {
                case EncryptionStrength.Sha256:
                    return HmacSha256Hash.GetHash(source, hkey);
                case EncryptionStrength.Sha512:
                    return HmacSha512Hash.GetHash(source, hkey);

                default:
                    throw new ArgumentOutOfRangeException(nameof(strength), strength, null);
            }
        }

        public int RandomNumber(int startInclusive, int endExclusive)
        {
            if (startInclusive > endExclusive)
                throw new ArgumentOutOfRangeException(nameof(startInclusive));
            if (startInclusive == endExclusive) return startInclusive;
            long diff = endExclusive - startInclusive;
            while (true)
            {
                RngCryptoServiceProvider.GetBytes(_uint32Buffer);
                uint rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                long max = (1 + (long)uint.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (int)(startInclusive + (rand % diff));
                }
            }
        }

        public string GenerateRandomKey(EncryptionStrength strength, bool alphaNumericOnly = true)
        {
            switch (strength)
            {
                case EncryptionStrength.Sha256:
                    var hmac256 = new HMACSHA256();
                    string key256 = Convert.ToBase64String(hmac256.Key);
                    if (alphaNumericOnly)
                    {
                        key256 = string.Join("", key256.Where(char.IsLetterOrDigit));
                    }
                    return key256;

                case EncryptionStrength.Sha512:
                    var hmac512 = new HMACSHA512();
                    string key512 = Convert.ToBase64String(hmac512.Key);
                    if (alphaNumericOnly)
                    {
                        key512 = string.Join("", key512.Where(char.IsLetterOrDigit));

                    }

                    return key512;

                default:
                    throw new ArgumentOutOfRangeException(nameof(strength), strength, null);
            }
        }
    }
}