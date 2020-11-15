using ApplicationManager.Hasher.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ApplicationManager.Hasher
{
    public class Manager
    {
        /// <summary>
        /// What trying to compare to
        /// </summary>
        private readonly byte[] _expectedHash;

        /// <summary>
        /// 
        /// </summary>
        private readonly HashAlgorithm _algorithm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expectedHash"></param>
        /// <param name="algorithm"></param>
        private Manager(byte[] expectedHash, HashAlgorithm algorithm)
        {
            _expectedHash = expectedHash;
            _algorithm = algorithm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        internal static Manager Get(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                throw new InvalidHashFormatException("Hash string is empty.");
            }

            var parts = hash.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new InvalidHashFormatException("Hash string has invalid format. Expected 'type:value'.");
            }

            var expectedHash = GetFromHex(parts[1]);
            var expectedType = parts[0].ToLower();
            return expectedType switch
            {
                "md5" => new Manager(expectedHash, MD5.Create()),
                "sha256" => new Manager(expectedHash, SHA256.Create()),
                "sha512" => new Manager(expectedHash, SHA512.Create()),
                _ => throw new InvalidHashTypeException(expectedType, "Unrecognized hash type. It's not implemented yet."),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal bool Validate(Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidStreamTypeException("Cannot validate the stream because of not allowed 'seek' method.");
            }

            var hash = _algorithm.ComputeHash(stream);
            if (hash.Length != _expectedHash.Length)
            {
                return false;
            }

            for (var i = 0; i < hash.Length; i++)
            {
                if (hash[i] != _expectedHash[i])
                {
                    return false;
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private static byte[] GetFromHex(string hash)
        {
            return Enumerable.Range(0, hash.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hash.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
