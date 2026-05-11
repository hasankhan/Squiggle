using System;
using System.Collections.Generic;
using System.Threading;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
using Squiggle.VoiceChat.Audio;

namespace Squiggle.VoiceChat
{
    class VoiceChatHandler : ActivityHandler, IVoiceChatHandler
    {
        private readonly IAudioCapture _capture;
        private readonly IAudioPlayback _playback;
        private readonly IAudioCodec _codec;

        public bool IsMuted { get; set; }

        public float Volume
        {
            get => _playback.Volume;
            set => _playback.Volume = Math.Max(0, Math.Min(value, 1));
        }

        public VoiceChatHandler(IActivityExecutor executor)
            : this(executor, AudioFactory.CreateCapture(), AudioFactory.CreatePlayback(), AudioFactory.CreateCodec())
        {
        }

        public VoiceChatHandler(IActivityExecutor executor, IAudioCapture capture, IAudioPlayback playback, IAudioCodec codec)
            : base(executor)
        {
            _capture = capture;
            _playback = playback;
            _codec = codec;
        }

        public override IDictionary<string, string> CreateInviteMetadata()
        {
            return new Dictionary<string, string>();
        }

        public override void TransferData(Func<bool> cancelPending)
        {
            // Thread.Sleep is intentional here: this runs on a background thread inside
            // ActivityExecutor's Task.Run and simply keeps the voice chat session alive
            // until cancellation. Converting to async would require changing the
            // ActivityHandler.TransferData contract across all implementations.
            while (!cancelPending())
                Thread.Sleep(100);
        }

        public override void OnDataReceived(byte[] chunk)
        {
            byte[] decoded = _codec.Decode(chunk, 0, chunk.Length);
            _playback.AddSamples(decoded, 0, decoded.Length);
        }

        public override void OnTransferStarted()
        {
            _capture.DataAvailable += OnCaptureDataAvailable;
            _capture.Start(_codec.SampleRate, _codec.Channels, _codec.BitsPerSample);
            _playback.Start(_codec.SampleRate, _codec.Channels, _codec.BitsPerSample);

            base.OnTransferStarted();
        }

        public override void OnTransferFinished()
        {
            _capture.DataAvailable -= OnCaptureDataAvailable;
            _capture.Stop();
            _playback.Stop();

            _codec.Dispose();
            _capture.Dispose();
            _playback.Dispose();

            base.OnTransferFinished();
        }

        private void OnCaptureDataAvailable(object? sender, AudioDataEventArgs e)
        {
            byte[] buffer = IsMuted ? GetEmptyBuffer(e.BytesRecorded) : e.Buffer;
            byte[] encoded = _codec.Encode(buffer, 0, e.BytesRecorded);
            SendData(encoded);
        }

        private byte[]? _emptyBuffer;
        private byte[] GetEmptyBuffer(int size)
        {
            if (_emptyBuffer == null || _emptyBuffer.Length < size)
                _emptyBuffer = new byte[size];
            return _emptyBuffer;
        }
    }
}
