using System;

namespace Squiggle.VoiceChat.Audio;

/// <summary>
/// Stub playback implementation for platforms without audio support.
/// </summary>
public class NullAudioPlayback : IAudioPlayback
{
    public float Volume { get; set; } = 1.0f;

    public void Start(int sampleRate, int channels, int bitsPerSample) { }
    public void AddSamples(byte[] buffer, int offset, int count) { }
    public void Stop() { }
    public void Dispose() { }
}
