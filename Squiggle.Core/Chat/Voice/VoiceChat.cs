using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using NAudio.Wave;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;

namespace Squiggle.Core.Chat.Voice
{
    class VoiceChat: AppHandler, IVoiceChat
    {
        WaveIn waveIn;
        WaveOut waveOut;
        EchoFilterWaveProvider waveProvider;
        AcmChatCodec codec = new Gsm610ChatCodec();

        public override Guid AppId
        {
            get { return ChatApps.VoiceChat; }
        }

        public Dispatcher Dispatcher { get; set; }

        public bool IsMuted { get; set; }

        public VoiceChat(Guid sessionId, ChatHost chatHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
            :base(sessionId, chatHost, localUser, remoteUser)
        {
        }

        public VoiceChat(Guid sessionId, ChatHost chatHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, Guid appSessionId)
            :base(sessionId, chatHost, localUser, remoteUser, appSessionId)
        {
        }

        protected override IEnumerable<KeyValuePair<string, string>> CreateInviteMetadata()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public float Volume
        {
            get { return waveOut.Coalesce(w=>w.Volume, 0); }
            set
            {
                if (waveOut != null)
                    waveOut.Volume = Math.Max(0, Math.Min(value, 1));
            }
        }

        protected override void TransferData(Func<bool> cancelPending)
        {
            while (!cancelPending())
                Thread.Sleep(100);
        }

        protected override void OnDataReceived(byte[] chunk)
        {
            byte[] decoded = codec.Decode(chunk, 0, chunk.Length);
            waveProvider.AddRemoteSamples(decoded, 0, decoded.Length);
        }

        protected override void OnTransferStarted()
        {
            Dispatcher.Invoke(() =>
            {
                waveIn = new WaveIn();
                waveIn.BufferMilliseconds = 50;
                waveIn.DeviceNumber = -1;
                waveIn.WaveFormat = codec.RecordFormat;
                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.StartRecording();

                waveOut = new WaveOut();
                int frameSize = 128;
                int filterLength = frameSize * 10;
                waveProvider = new EchoFilterWaveProvider(codec.RecordFormat, frameSize, filterLength);
                waveOut.Init(waveProvider);
                waveOut.Play();
            });

            base.OnTransferStarted();
        }

        public new void Accept()
        {
            base.Accept();
        }

        protected override void OnTransferFinished()
        {
            Dispatcher.Invoke(() =>
            {
                if (waveIn != null)
                {
                    waveIn.DataAvailable -= waveIn_DataAvailable;
                    waveIn.StopRecording();
                    waveOut.Stop();

                    codec.Dispose();                   
                    waveProvider.Dispose();
                    waveIn.Dispose();
                    waveOut.Dispose();
                }
            });

            base.OnTransferFinished();
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = IsMuted ? GetEmptyBuffer(e.BytesRecorded) : e.Buffer;
         
            waveProvider.AddLocalSamples(buffer, 0, e.BytesRecorded);
            byte[] encoded = codec.Encode(buffer, 0, e.BytesRecorded);
            SendData(encoded);
        }

        byte[] emptyBuffer;
        byte[] GetEmptyBuffer(int size)
        {
            if (emptyBuffer == null || emptyBuffer.Length < size)
                emptyBuffer = new byte[size];
            return emptyBuffer;
        }
    }
}
