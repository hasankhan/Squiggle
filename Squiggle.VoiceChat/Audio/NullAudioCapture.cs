using System;

namespace Squiggle.VoiceChat.Audio;

/// <summary>
/// Stub capture implementation for platforms without audio support.
/// </summary>
public class NullAudioCapture : IAudioCapture
{
    public event EventHandler<AudioDataEventArgs> DataAvailable = delegate { };

    public void Start(int sampleRate, int channels, int bitsPerSample) { }
    public void Stop() { }
    public void Dispose() { }
}
