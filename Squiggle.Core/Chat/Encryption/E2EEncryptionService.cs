using System;
using System.Security.Cryptography;

namespace Squiggle.Core.Chat.Encryption
{
    /// <summary>
    /// Provides end-to-end encryption using ECDH (NIST P-256) key exchange
    /// and AES-256-GCM authenticated encryption.
    /// </summary>
    public sealed class E2EEncryptionService : IDisposable
    {
        const int NonceSizeBytes = 12;
        const int TagSizeBytes = 16;
        const int KeySizeBytes = 32;

        static readonly byte[] HkdfInfo = "squiggle-e2ee-chat-v1"u8.ToArray();

        readonly ECDiffieHellman ecdh;

        public E2EEncryptionService()
        {
            ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        }

        /// <summary>
        /// Returns the local public key for exchange with a peer.
        /// </summary>
        public byte[] GetPublicKey()
        {
            return ecdh.PublicKey.ExportSubjectPublicKeyInfo();
        }

        /// <summary>
        /// Derives a shared AES-256 key from the peer's public key using ECDH + HKDF.
        /// </summary>
        public byte[] DeriveSharedKey(byte[] peerPublicKeyBytes)
        {
            using var peerKey = ECDiffieHellman.Create();
            peerKey.ImportSubjectPublicKeyInfo(peerPublicKeyBytes, out _);

            // Raw ECDH shared secret
            byte[] sharedSecret = ecdh.DeriveRawSecretAgreement(peerKey.PublicKey);
            try
            {
                // Derive a 256-bit key via HKDF-SHA256
                return HKDF.DeriveKey(
                    HashAlgorithmName.SHA256,
                    sharedSecret,
                    KeySizeBytes,
                    salt: null,
                    info: HkdfInfo);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }
        }

        /// <summary>
        /// Encrypts plaintext using AES-256-GCM with a random 12-byte nonce.
        /// Returns (ciphertext, nonce). The authentication tag is appended to the ciphertext.
        /// </summary>
        public static (byte[] Ciphertext, byte[] Nonce) Encrypt(byte[] plaintext, byte[] sharedKey)
        {
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[TagSizeBytes];

            using var aes = new AesGcm(sharedKey, TagSizeBytes);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // Append tag to ciphertext for transport: [ciphertext || tag]
            byte[] result = new byte[ciphertext.Length + tag.Length];
            Buffer.BlockCopy(ciphertext, 0, result, 0, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, ciphertext.Length, tag.Length);

            return (result, nonce);
        }

        /// <summary>
        /// Decrypts ciphertext (with appended tag) using AES-256-GCM.
        /// </summary>
        public static byte[] Decrypt(byte[] ciphertextWithTag, byte[] nonce, byte[] sharedKey)
        {
            if (ciphertextWithTag.Length < TagSizeBytes)
                throw new CryptographicException("Ciphertext too short — missing authentication tag.");

            int ciphertextLength = ciphertextWithTag.Length - TagSizeBytes;
            byte[] ciphertext = ciphertextWithTag.AsSpan(0, ciphertextLength).ToArray();
            byte[] tag = ciphertextWithTag.AsSpan(ciphertextLength, TagSizeBytes).ToArray();
            byte[] plaintext = new byte[ciphertextLength];

            using var aes = new AesGcm(sharedKey, TagSizeBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return plaintext;
        }

        /// <summary>
        /// Computes a SHA-256 fingerprint of a public key for manual verification.
        /// Returns a hex string like "AB:CD:EF:..."
        /// </summary>
        public static string ComputeFingerprint(byte[] publicKey)
        {
            byte[] hash = SHA256.HashData(publicKey);
            // Use first 16 bytes (128 bits) for a readable fingerprint
            return BitConverter.ToString(hash, 0, 16).Replace("-", ":");
        }

        public void Dispose()
        {
            ecdh.Dispose();
        }
    }
}
