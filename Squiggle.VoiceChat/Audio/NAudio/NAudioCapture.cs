using System;
using System.Runtime.Versioning;
using NAudio.Wave;

namespace Squiggle.VoiceChat.Audio.NAudio;

[SupportedOSPlatform("windows")]
public class NAudioCapture : IAudioCapture
{
    private WaveIn? _waveIn;

    public event EventHandler<AudioDataEventArgs> DataAvailable = delegate { };

    public void Start(int sampleRate, int channels, int bitsPerSample)
    {
        _waveIn = new WaveIn
        {
            BufferMilliseconds = 50,
            DeviceNumber = -1,
            WaveFormat = new WaveFormat(sampleRate, bitsPerSample, channels)
        };
        _waveIn.DataAvailable += (s, e) =>
            DataAvailable.Invoke(this, new AudioDataEventArgs(e.Buffer, e.BytesRecorded));
        _waveIn.StartRecording();
    }

    public void Stop()
    {
        _waveIn?.StopRecording();
    }

    public void Dispose()
    {
        _waveIn?.Dispose();
        _waveIn = null;
    }
}
