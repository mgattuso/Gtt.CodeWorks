using System;
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

        public async Task<EncryptedData> Encrypt(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new EncryptedData {IsEmpty = true};
            }

            var hash = await Hash(str, false);

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

        public async Task<string> Decrypt(string str)
        {
            await Task.CompletedTask;

            byte[] ivPlusMessage = Convert.FromBase64String(str);
            byte[] message = new byte[ivPlusMessage.Length - 16];
            byte[] iv = new byte[16];

            Array.ConstrainedCopy(ivPlusMessage, 0, iv, 0, 16);
            Array.ConstrainedCopy(ivPlusMessage, 16, message, 0, ivPlusMessage.Length - 16);

            var ekey = _encryptionKey;

            var decrypted = AesEncryption.DecryptStringFromBytes_Aes(message, Convert.FromBase64String(ekey), iv);
            return decrypted;
        }

        public async Task<string> Hash(string source, bool caseInsensitive)
        {
            await Task.CompletedTask;
            var hkey = _hashKey;

            if (caseInsensitive)
            {
                source = source?.ToUpperInvariant();
            }

            return HmacSha256Hash.GetHash(source, hkey);
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

                long max = (1 + (long) uint.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (int) (startInclusive + (rand % diff));
                }
            }
        }

        public async Task<string> CreateKey()
        {
            await Task.CompletedTask;
            var hmac = new HMACSHA256();
            for (int i = 0; i < 100; i++)
            {
                string key = Convert.ToBase64String(hmac.Key);
                if (!key.Contains("/"))
                {
                    return key;
                }
            }

            throw new Exception("Could not generate an acceptable key");
        }
    }
}