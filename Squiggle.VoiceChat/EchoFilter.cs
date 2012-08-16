using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Squiggle.VoiceChat
{
    class EchoFilter : IDisposable
    {
        [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr speex_echo_state_init(int frame_size, int filter_length);

        [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void speex_echo_cancellation(IntPtr state, byte[] inputFrame, byte[] echoFrame, byte[] outputFrame);

        [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void speex_echo_state_destroy(IntPtr state);

        IntPtr state;

        /// <param name="frameSize">frameSize is the amount of data (in samples) you want to process at once.</param>
        /// <param name="filterLength">filterLength is the length (in samples) of the echo cancelling filter you want to use (also known as tail length).</param>
        public EchoFilter(int frameSize, int filterLength)
        {
            state = speex_echo_state_init(frameSize, filterLength);
        }

        /// <summary>
        /// Method for echo cancellation
        /// </summary>
        /// <param name="inputFrame">Frame obtained from local microphone (Signal that contains echo)</param>
        /// <param name="echoFrame">Frame obtained from remote source (Source of echo)</param>
        /// <param name="outputFrame">Filtered output</param>
        public void Filter(byte[] inputFrame, byte[] echoFrame, byte[] outputFrame)
        {
            speex_echo_cancellation(state, inputFrame, echoFrame, outputFrame);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (state != IntPtr.Zero)
            {
                speex_echo_state_destroy(state);
                state = IntPtr.Zero;
            }
        }

        ~EchoFilter()
        {
            Dispose(false);
        }
    }
}
