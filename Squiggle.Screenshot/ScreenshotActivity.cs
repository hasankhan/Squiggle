using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Squiggle.FileTransfer;
using Squiggle.Plugins;

namespace Squiggle.Screenshot
{
    public class ScreenshotActivity : FileTransferActivity
    {
        private readonly IScreenCaptureService _captureService = ScreenCaptureFactory.Create();

        public override string Title => "Screenshot";

        public override async Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window)
        {
            string fileName = string.Format("Screenshot_{0:yyMMddHHmmss}.jpg", DateTime.Now);

            window.Hide();
            await Task.Delay(TimeSpan.FromSeconds(1));

            Stream? stream = _captureService.CaptureFullScreen();
            window.Restore();

            if (stream == null)
                return new Dictionary<string, object>();

            var args = new Dictionary<string, object>()
            {
                { "name", fileName },
                { "content", stream },
                { "size", stream.Length }
            };
            stream.Seek(0, SeekOrigin.Begin);

            return args;
        }
    }
}
