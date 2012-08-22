using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Squiggle.Core.Chat
{
    public class TextMessageReceivedEventArgs : SessionEventArgs
    {
        public string Id { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }
}
