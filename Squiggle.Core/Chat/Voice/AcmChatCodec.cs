using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.Compression;

namespace Squiggle.Core.Chat.Voice
{
    abstract class AcmChatCodec
    {
        private WaveFormat encodeFormat;
        private AcmStream encodeStream;
        private AcmStream decodeStream;
        private int decodeSourceBytesLeftovers;
        private int encodeSourceBytesLeftovers;

        public AcmChatCodec(WaveFormat recordFormat, WaveFormat encodeFormat)
        {
            this.RecordFormat = recordFormat;
            this.encodeFormat = encodeFormat;
        }

        public WaveFormat RecordFormat { get; private set; }

        public byte[] Encode(byte[] data, int offset, int length)
        {
            if (this.encodeStream == null)
                this.encodeStream = new AcmStream(this.RecordFormat, this.encodeFormat);
            return Convert(encodeStream, data, offset, length, ref encodeSourceBytesLeftovers);
        }

        public byte[] Decode(byte[] data, int offset, int length)
        {
            if (this.decodeStream == null)
                this.decodeStream = new AcmStream(this.encodeFormat, this.RecordFormat);
            return Convert(decodeStream, data, offset, length, ref decodeSourceBytesLeftovers);
        }

        private static byte[] Convert(AcmStream conversionStream, byte[] data, int offset, int length, ref int sourceBytesLeftovers)
        {
            int bytesInSourceBuffer = length + sourceBytesLeftovers;
            Array.Copy(data, offset, conversionStream.SourceBuffer, sourceBytesLeftovers, length);
            int sourceBytesConverted;
            int bytesConverted = conversionStream.Convert(bytesInSourceBuffer, out sourceBytesConverted);
            sourceBytesLeftovers = bytesInSourceBuffer - sourceBytesConverted;
            if (sourceBytesLeftovers > 0)
            {
                //Debug.WriteLine(String.Format("Asked for {0}, converted {1}", bytesInSourceBuffer, sourceBytesConverted));
                // shift the leftovers down
                Array.Copy(conversionStream.SourceBuffer, sourceBytesConverted, conversionStream.SourceBuffer, 0, sourceBytesLeftovers);
            }
            byte[] encoded = new byte[bytesConverted];
            Array.Copy(conversionStream.DestBuffer, 0, encoded, 0, bytesConverted);
            return encoded;
        }

        public abstract string Name { get; }

        public int BitsPerSecond
        {
            get
            {
                return this.encodeFormat.AverageBytesPerSecond * 8;
            }
        }

        public void Dispose()
        {
            if (encodeStream != null)
            {
                encodeStream.Dispose();
                encodeStream = null;
            }
            if (decodeStream != null)
            {
                decodeStream.Dispose();
                decodeStream = null;
            }
        }

        public bool IsAvailable
        {
            get
            {
                // determine if this codec is installed on this PC
                bool available = true;
                try
                {
                    using (var tempEncoder = new AcmStream(this.RecordFormat, this.encodeFormat)) { }
                    using (var tempDecoder = new AcmStream(this.encodeFormat, this.RecordFormat)) { }
                }
                catch (MmException)
                {
                    available = false;
                }
                return available;
            }
        }
    }
}
