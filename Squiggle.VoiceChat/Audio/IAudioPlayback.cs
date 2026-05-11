using System;

namespace Squiggle.VoiceChat.Audio;

public interface IAudioPlayback : IDisposable
{
    void Start(int sampleRate, int channels, int bitsPerSample);
    void AddSamples(byte[] buffer, int offset, int count);
    void Stop();
    float Volume { get; set; }
}
