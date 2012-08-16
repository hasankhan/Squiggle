using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace Squiggle.VoiceChat
{
    class EchoFilterWaveProvider : IWaveProvider, IDisposable
    {
        BufferedWaveProvider localSound;
        BufferedWaveProvider remoteSound;
        BufferedWaveProvider filtered;
        int bytesPerFrame;
        EchoFilter filter;
        byte[] remoteFrame;
        byte[] localFrame;
        byte[] outputFrame;
        object syncRoot = new object();

        /// <param name="format">Wave format</param>
        /// <param name="frameSize">frameSize is the amount of data (in samples) you want to process at once.</param>
        /// <param name="filterLength">filterLength is the length (in samples) of the echo cancelling filter you want to use (also known as tail length).</param>
        public EchoFilterWaveProvider(WaveFormat format, int frameSize, int filterLength)
        {
            bytesPerFrame = format.BitsPerSample / 8 * frameSize;
            remoteFrame = new byte[bytesPerFrame];
            localFrame = new byte[bytesPerFrame];
            outputFrame = new byte[bytesPerFrame];

            filter = new EchoFilter(frameSize, filterLength);
            localSound = new BufferedWaveProvider(format) { DiscardOnBufferOverflow = true };
            remoteSound = new BufferedWaveProvider(format) { DiscardOnBufferOverflow = true };
            filtered = new BufferedWaveProvider(format) { DiscardOnBufferOverflow = true };
        }

        public void AddLocalSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                localSound.AddSamples(buffer, offset, count);
        }

        public void AddRemoteSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                remoteSound.AddSamples(buffer, offset, count);
        }        

        public WaveFormat WaveFormat
        {
            get { return remoteSound.WaveFormat; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
            {
                while (remoteSound.BufferedBytes >= bytesPerFrame && localSound.BufferedBytes >= bytesPerFrame)
                {
                    // read source of echo
                    remoteSound.Read(remoteFrame, 0, bytesPerFrame);
                    // read local sound + echo
                    localSound.Read(localFrame, 0, bytesPerFrame);

                    filter.Filter(localFrame, remoteFrame, outputFrame);
                    filtered.AddSamples(outputFrame, 0, outputFrame.Length);
                }
                return filtered.Read(buffer, offset, count);
            }
        }

        public void Dispose()
        {
            filter.Dispose();
        }
    }
}
