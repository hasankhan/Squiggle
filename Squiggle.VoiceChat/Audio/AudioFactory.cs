using System;

namespace Squiggle.VoiceChat.Audio;

public static class AudioFactory
{
    public static IAudioCapture CreateCapture()
    {
        if (OperatingSystem.IsWindows())
            return new NAudio.NAudioCapture();
        return new NullAudioCapture();
    }

    public static IAudioPlayback CreatePlayback()
    {
        if (OperatingSystem.IsWindows())
            return new NAudio.NAudioPlayback();
        return new NullAudioPlayback();
    }

    public static IAudioCodec CreateCodec()
    {
        // TODO: Add Opus codec via Concentus NuGet package for cross-platform compression
        return new PcmCodec();
    }
}
