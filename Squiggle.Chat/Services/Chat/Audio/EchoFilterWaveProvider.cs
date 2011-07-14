using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace Squiggle.Chat.Services.Chat.Audio
{
    class EchoFilterWaveProvider : IWaveProvider, IDisposable
    {
        BufferedWaveProvider localSound;
        BufferedWaveProvider remoteSoundAndEcho;
        BufferedWaveProvider filtered;
        int frameBytes;
        EchoFilter filter;
        byte[] remoteFrame;
        byte[] localFrame;
        byte[] outputFrame;
        object syncRoot = new object();

        public EchoFilterWaveProvider(WaveFormat format, int frameSize, int filterLength)
        {
            frameBytes = frameSize * 2;
            remoteFrame = new byte[frameBytes];
            localFrame = new byte[frameBytes];
            outputFrame = new byte[frameBytes];

            filter = new EchoFilter(frameSize, filterLength);
            localSound = new BufferedWaveProvider(format);
            remoteSoundAndEcho = new BufferedWaveProvider(format);
            filtered = new BufferedWaveProvider(format);
            localSound.DiscardOnBufferOverflow = remoteSoundAndEcho.DiscardOnBufferOverflow 
                                             = filtered.DiscardOnBufferOverflow 
                                             = true;
        }

        public void AddLocalSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                localSound.AddSamples(buffer, offset, count);
        }

        public void AddRemoteSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                remoteSoundAndEcho.AddSamples(buffer, offset, count);
        }        

        public WaveFormat WaveFormat
        {
            get { return remoteSoundAndEcho.WaveFormat; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
            {
                while (remoteSoundAndEcho.BufferedBytes >= frameBytes && localSound.BufferedBytes >= frameBytes)
                {
                    remoteSoundAndEcho.Read(remoteFrame, 0, frameBytes);
                    localSound.Read(localFrame, 0, frameBytes);
                    filter.Filter(remoteFrame, localFrame, outputFrame);
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
