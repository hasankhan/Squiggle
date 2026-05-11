using System.IO;

namespace Squiggle.Screenshot;

/// <summary>
/// Stub implementation for non-Windows platforms where screen capture is not available.
/// </summary>
public class NullScreenCaptureService : IScreenCaptureService
{
    public Stream? CaptureFullScreen() => null;
}
