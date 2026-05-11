using System;
using System.Runtime.Versioning;
using NAudio.Wave;

namespace Squiggle.VoiceChat.Audio.NAudio;

[SupportedOSPlatform("windows")]
public class NAudioPlayback : IAudioPlayback
{
    private WaveOut? _waveOut;
    private BufferedWaveProvider? _buffer;

    public float Volume
    {
        get => _waveOut?.Volume ?? 0f;
        set
        {
            if (_waveOut != null)
                _waveOut.Volume = Math.Max(0, Math.Min(value, 1));
        }
    }

    public void Start(int sampleRate, int channels, int bitsPerSample)
    {
        var format = new WaveFormat(sampleRate, bitsPerSample, channels);
        _buffer = new BufferedWaveProvider(format) { DiscardOnBufferOverflow = true };
        _waveOut = new WaveOut();
        _waveOut.Init(_buffer);
        _waveOut.Play();
    }

    public void AddSamples(byte[] buffer, int offset, int count)
    {
        _buffer?.AddSamples(buffer, offset, count);
    }

    public void Stop()
    {
        _waveOut?.Stop();
    }

    public void Dispose()
    {
        _waveOut?.Dispose();
        _waveOut = null;
        _buffer = null;
    }
}
