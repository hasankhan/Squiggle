using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Squiggle.Core.Chat.Encryption
{
    /// <summary>
    /// Manages per-peer encryption sessions: key pairs, public key exchange,
    /// and shared key derivation. Thread-safe.
    /// </summary>
    public sealed class EncryptionManager : IDisposable
    {
        readonly ConcurrentDictionary<string, PeerEncryptionState> peerStates = new();
        readonly ILogger logger;

        public EncryptionManager(ILogger? logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Gets or creates the encryption state for a peer, generating our local key pair.
        /// Returns our public key bytes to send to the peer.
        /// </summary>
        public byte[] GetOrCreateLocalPublicKey(string peerId)
        {
            var state = peerStates.GetOrAdd(peerId, _ =>
            {
                logger.LogDebug("Creating new encryption session for peer {PeerId}", peerId);
                return new PeerEncryptionState();
            });
            return state.Service.GetPublicKey();
        }

        /// <summary>
        /// Processes a peer's public key, deriving the shared secret.
        /// Returns our public key if we haven't sent one yet (for the response leg).
        /// </summary>
        public byte[]? OnPeerPublicKeyReceived(string peerId, byte[] peerPublicKey)
        {
            var state = peerStates.GetOrAdd(peerId, _ =>
            {
                logger.LogDebug("Creating new encryption session for peer {PeerId} (initiated by peer)", peerId);
                return new PeerEncryptionState();
            });

            if (state.SharedKey != null)
            {
                logger.LogDebug("Shared key already derived for peer {PeerId}, ignoring duplicate key exchange", peerId);
                return null;
            }

            state.SharedKey = state.Service.DeriveSharedKey(peerPublicKey);
            logger.LogInformation("E2EE shared key derived for peer {PeerId}, fingerprint={Fingerprint}",
                peerId, E2EEncryptionService.ComputeFingerprint(peerPublicKey));

            // Return our public key so the caller can send it back if needed
            return state.NeedsSendKey ? state.Service.GetPublicKey() : null;
        }

        /// <summary>
        /// Marks that we've sent our key to this peer (so we don't send it again).
        /// </summary>
        public void MarkKeySent(string peerId)
        {
            if (peerStates.TryGetValue(peerId, out var state))
                state.NeedsSendKey = false;
        }

        /// <summary>
        /// Returns true if encryption is established (shared key derived) for this peer.
        /// </summary>
        public bool IsEncrypted(string peerId)
        {
            return peerStates.TryGetValue(peerId, out var state) && state.SharedKey != null;
        }

        /// <summary>
        /// Encrypts data for a specific peer. Returns null if encryption is not yet established.
        /// </summary>
        public (byte[] Ciphertext, byte[] Nonce)? Encrypt(string peerId, byte[] plaintext)
        {
            if (!peerStates.TryGetValue(peerId, out var state) || state.SharedKey == null)
                return null;

            return E2EEncryptionService.Encrypt(plaintext, state.SharedKey);
        }

        /// <summary>
        /// Decrypts data from a specific peer. Returns null if encryption is not established.
        /// </summary>
        public byte[]? Decrypt(string peerId, byte[] ciphertext, byte[] nonce)
        {
            if (!peerStates.TryGetValue(peerId, out var state) || state.SharedKey == null)
            {
                logger.LogWarning("Cannot decrypt — no shared key for peer {PeerId}", peerId);
                return null;
            }

            return E2EEncryptionService.Decrypt(ciphertext, nonce, state.SharedKey);
        }

        /// <summary>
        /// Removes encryption state for a peer (e.g., when session ends).
        /// </summary>
        public void RemovePeer(string peerId)
        {
            if (peerStates.TryRemove(peerId, out var state))
            {
                state.Dispose();
                logger.LogDebug("Removed encryption state for peer {PeerId}", peerId);
            }
        }

        public void Dispose()
        {
            foreach (var kvp in peerStates)
                kvp.Value.Dispose();
            peerStates.Clear();
        }

        sealed class PeerEncryptionState : IDisposable
        {
            public E2EEncryptionService Service { get; } = new();
            public byte[]? SharedKey { get; set; }
            public bool NeedsSendKey { get; set; } = true;

            public void Dispose() => Service.Dispose();
        }
    }
}
