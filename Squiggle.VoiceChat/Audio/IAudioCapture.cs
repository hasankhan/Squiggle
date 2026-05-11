using System;

namespace Squiggle.VoiceChat.Audio;

public interface IAudioCapture : IDisposable
{
    event EventHandler<AudioDataEventArgs> DataAvailable;
    void Start(int sampleRate, int channels, int bitsPerSample);
    void Stop();
}
