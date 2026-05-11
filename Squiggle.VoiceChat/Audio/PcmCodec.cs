using System;

namespace Squiggle.VoiceChat.Audio;

/// <summary>
/// Pass-through codec that sends raw PCM data without compression.
/// TODO: Replace with Opus codec via Concentus NuGet package for cross-platform compression.
/// </summary>
public class PcmCodec : IAudioCodec
{
    public int SampleRate => 16000;
    public int Channels => 1;
    public int BitsPerSample => 16;

    public byte[] Encode(byte[] data, int offset, int length)
    {
        var result = new byte[length];
        Array.Copy(data, offset, result, 0, length);
        return result;
    }

    public byte[] Decode(byte[] data, int offset, int length)
    {
        var result = new byte[length];
        Array.Copy(data, offset, result, 0, length);
        return result;
    }

    public void Dispose() { }
}
