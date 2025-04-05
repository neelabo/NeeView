#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace NeeView
{
    /// <summary>
    /// アーカイブキーキャッシュ
    /// </summary>
    /// <remarks>
    /// あまり意味はないがキャッシュを暗号化している。
    /// このキャッシュは保存されない。
    /// </remarks>
    [LocalDebug]
    public partial class ArchiveKeyCache
    {
        public static ArchiveKeyCache Current { get; } = new();

        private readonly Dictionary<string, byte[]> _map = new();
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIV;

        public ArchiveKeyCache()
        {
            var aes = Aes.Create();
            _aesKey = aes.Key;
            _aesIV = aes.IV;
        }

        public void Clear()
        {
            _map.Clear();
        }

        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            LocalDebug.WriteLine($"Key={key}, Value={value}");
            _map[key] = Encrypt(value, _aesKey, _aesIV);
        }

        public bool Remove(string key)
        {
            LocalDebug.WriteLine($"Key={key}");
            return _map.Remove(key);
        }

        public string GetValue(string key)
        {
            if (TryGetValue(key, out string? value))
            {
                return value;
            }
            return "";
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            if (_map.TryGetValue(key, out var encrypted))
            {
                value = Decrypt(encrypted, _aesKey, _aesIV);
                LocalDebug.WriteLine($"Key={key}, Value={value}");
                return true;
            }
            else
            {
                value = null;
                LocalDebug.WriteLine($"Key={key} not found.");
                return false;
            }
        }

        private static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException(nameof(plainText));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        private static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException(nameof(cipherText));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
