using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using NAudio.Wave;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.VoiceChat
{
    class VoiceChatHandler: ActivityHandler, IVoiceChatHandler
    {
        WaveIn waveIn;
        WaveOut waveOut;
        BufferedWaveProvider waveOutProvider;
        EchoFilterWaveProvider echoFilter;
        AcmChatCodec codec = new Gsm610ChatCodec();

        public Dispatcher Dispatcher { get; set; }

        public bool IsMuted { get; set; }

        public VoiceChatHandler(IActivityExecutor executor) : base(executor) { }

        public override IDictionary<string, string> CreateInviteMetadata()
        {
            return new Dictionary<string, string>();
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

        public override void TransferData(Func<bool> cancelPending)
        {
            while (!cancelPending())
                Thread.Sleep(100);
        }

        public override void OnDataReceived(byte[] chunk)
        {
            byte[] decoded = codec.Decode(chunk, 0, chunk.Length);
            waveOutProvider.AddSamples(decoded, 0, decoded.Length);
            echoFilter.AddRemoteSamples(decoded, 0, decoded.Length);
        }

        public override void OnTransferStarted()
        {
            Dispatcher.Invoke(() =>
            {
                CreateEchoFilter();

                waveIn = new WaveIn();
                waveIn.BufferMilliseconds = 50;
                waveIn.DeviceNumber = -1;
                waveIn.WaveFormat = codec.RecordFormat;
                waveIn.DataAvailable += waveIn_DataAvailable;
                waveIn.StartRecording();

                waveOut = new WaveOut();
                waveOutProvider = new BufferedWaveProvider(codec.RecordFormat) { DiscardOnBufferOverflow = true };
                waveOut.Init(waveOutProvider);
                waveOut.Play();
            });

            base.OnTransferStarted();
        }

        void CreateEchoFilter()
        {
            /* Source: http://www.speex.org/docs/manual/speex-manual/node7.html
             * It is recommended to use a frame size in the order of 20 ms (or equal to the codec frame size) and make sure it is easy to perform an FFT of that size (powers of two are better than prime sizes).            
             */
            TimeSpan frameSizeTime = TimeSpan.FromMilliseconds(20);
            int frameSize = (int)Math.Ceiling(frameSizeTime.TotalSeconds * codec.RecordFormat.SampleRate);

            /* Source: http://www.speex.org/docs/manual/speex-manual/node7.html
             * The recommended tail length is approximately the third of the room reverberation time. 
             * For example, in a small room, reverberation time is in the order of 300 ms, so a tail length of 100 ms is a good choice (800 samples at 8000 Hz sampling rate).
             */
            TimeSpan tailLengthTime = TimeSpan.FromMilliseconds(100);
            int filterLength = (int)Math.Ceiling(tailLengthTime.TotalSeconds * codec.RecordFormat.SampleRate);
            
            echoFilter = new EchoFilterWaveProvider(codec.RecordFormat, frameSize, filterLength);
        }

        public override void OnTransferFinished()
        {
            Dispatcher.Invoke(() =>
            {
                if (waveIn != null)
                {
                    waveIn.DataAvailable -= waveIn_DataAvailable;
                    waveIn.StopRecording();
                    waveOut.Stop();

                    codec.Dispose();                   
                    echoFilter.Dispose();
                    waveIn.Dispose();
                    waveOut.Dispose();
                }
            });

            base.OnTransferFinished();
        }

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = IsMuted ? GetEmptyBuffer(e.BytesRecorded) : e.Buffer;

            echoFilter.AddLocalSamples(buffer, 0, e.BytesRecorded);
            int filteredBytes = echoFilter.Read(buffer, 0, buffer.Length);

            byte[] encoded = codec.Encode(buffer, 0, filteredBytes);
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
