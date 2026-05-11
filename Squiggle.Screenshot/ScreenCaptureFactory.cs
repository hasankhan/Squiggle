using System.Runtime.InteropServices;

namespace Squiggle.Screenshot;

public static class ScreenCaptureFactory
{
    public static IScreenCaptureService Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsScreenCaptureService();
        return new NullScreenCaptureService();
    }
}
