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
        static extern void speex_echo_cancellation(IntPtr state, byte[] recorded, byte[] played, byte[] output);

        [DllImport("libspeexdsp.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void speex_echo_state_destroy(IntPtr state);

        IntPtr state;

        public EchoFilter(int frameSize, int filterLength)
        {
            state = speex_echo_state_init(frameSize, filterLength);
        }

        public void Filter(byte[] remoteSoundAndEcho, byte[] localVoice, byte[] output)
        {
            speex_echo_cancellation(state, remoteSoundAndEcho, localVoice, output);
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
