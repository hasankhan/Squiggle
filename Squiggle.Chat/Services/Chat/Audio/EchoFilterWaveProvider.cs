using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace Squiggle.Chat.Services.Chat.Audio
{
    class EchoFilterWaveProvider : IWaveProvider, IDisposable
    {
        BufferedWaveProvider recorded;
        BufferedWaveProvider playing;
        BufferedWaveProvider filtered;
        int frameBytes;
        EchoFilter filter;
        byte[] playingFrame;
        byte[] recordedFrame;
        byte[] outputFrame;
        object syncRoot = new object();

        public EchoFilterWaveProvider(WaveFormat format, int frameSize, int filterLength)
        {
            frameBytes = frameSize * 2;
            playingFrame = new byte[frameBytes];
            recordedFrame = new byte[frameBytes];
            outputFrame = new byte[frameBytes];

            filter = new EchoFilter(frameSize, filterLength);
            recorded = new BufferedWaveProvider(format);
            playing = new BufferedWaveProvider(format);
            filtered = new BufferedWaveProvider(format);
            recorded.DiscardOnBufferOverflow = playing.DiscardOnBufferOverflow 
                                             = filtered.DiscardOnBufferOverflow 
                                             = true;
        }

        public void AddRecordedSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                recorded.AddSamples(buffer, offset, count);
        }

        public void AddPlaybackSamples(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
            {
                playing.AddSamples(buffer, offset, count);
                while (playing.BufferedBytes >= frameBytes && recorded.BufferedBytes >= frameBytes)
                {
                    playing.Read(playingFrame, 0, frameBytes);
                    recorded.Read(recordedFrame, 0, frameBytes);
                    filter.Filter(recordedFrame, playingFrame, outputFrame);
                    filtered.AddSamples(outputFrame, 0, outputFrame.Length);
                }
            }
        }

        public WaveFormat WaveFormat
        {
            get { return playing.WaveFormat; }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (syncRoot)
                return filtered.Read(buffer, offset, count);
        }

        public void Dispose()
        {
            filter.Dispose();
        }
    }
}
