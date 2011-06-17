using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Chat.Host;
using System.IO;
using NAudio.Wave;
using System.Threading;

namespace Squiggle.Chat.Services.Chat.Audio
{
    class VoiceChat: AppHandler, IVoiceChat
    {
        WaveIn waveIn;
        IWavePlayer waveOut;
        BufferedWaveProvider waveProvider;
        AcmChatCodec codec = new Gsm610ChatCodec();

        public override Guid AppId
        {
            get { return ChatApps.VoiceChat; }
        }

        public VoiceChat(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
            :base(sessionId, remoteHost, localHost, localUser, remoteUser)
        {
        }

        public VoiceChat(Guid sessionId, IChatHost remoteHost, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, Guid appSessionId)
            :base(sessionId, remoteHost, localHost, localUser, remoteUser, appSessionId)
        {
        }

        protected override IEnumerable<KeyValuePair<string, string>> CreateInviteMetadata()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        protected override void TransferData(Func<bool> cancelPending)
        {
            while (cancelPending())
                Thread.Sleep(100);
        }

        protected override void OnDataReceived(byte[] chunk)
        {
            byte[] decoded = codec.Decode(chunk, 0, chunk.Length);
            waveProvider.AddSamples(decoded, 0, decoded.Length);
        }

        protected override void OnTransferStarted()
        {
            base.OnTransferStarted();

            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 50;
            waveIn.DeviceNumber = -1;
            waveIn.WaveFormat = codec.RecordFormat;
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.StartRecording();

            waveOut = new WaveOut();
            waveProvider = new BufferedWaveProvider(codec.RecordFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        public new void Accept()
        {
            base.Accept();
        }

        protected override void OnTransferFinished()
        {
            base.OnTransferFinished();

            waveIn.DataAvailable -= waveIn_DataAvailable;
            waveIn.StopRecording();
            waveOut.Stop();

            waveIn.Dispose();
            waveOut.Dispose();
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] encoded = codec.Encode(e.Buffer, 0, e.BytesRecorded);
            SendData(encoded);
        }
    }
}
