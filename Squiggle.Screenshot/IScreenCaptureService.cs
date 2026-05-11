using System.IO;

namespace Squiggle.Screenshot;

/// <summary>
/// Abstraction for screen capture to enable cross-platform implementations.
/// </summary>
public interface IScreenCaptureService
{
    /// <summary>Captures the full primary screen. Returns image stream or null if unavailable.</summary>
    Stream? CaptureFullScreen();
}
