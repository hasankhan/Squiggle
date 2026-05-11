using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Squiggle.Core.Chat.Activity;
using Squiggle.FileTransfer;
using Squiggle.Plugins;
using Squiggle.Utilities.Threading;

namespace Squiggle.Screenshot
{
    public class ScreenshotActivity: FileTransferActivity
    {
        public override string Title
        {
            get { return "Screenshot"; }
        }

        public override async Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window)
        {
            string fileName = String.Format("Screenshot_{0:yyMMddHHmmss}.jpg", DateTime.Now);

            var chatWindow = ((Window)window);
            chatWindow.WindowState = WindowState.Minimized;

            await Task.Delay(TimeSpan.FromSeconds(1));
            Stream stream = CaptureScreen();
            window.Restore();

            var args = new Dictionary<string, object>() 
            { 
                { "name",  fileName }, 
                { "content", stream }, 
                { "size", stream.Length } 
            };
            stream.Seek(0, SeekOrigin.Begin);

            return args;
        }

        private Stream CaptureScreen()
        {
            return Screenshot.Capture();
        }
    }
}
