using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Transport.Messages
{
    class TextMessage : Message
    {
        public Guid Id { get; set; }
        public string FontName { get; set; } = null!;
        public int FontSize { get; set; }
        int ColorR { get; set; }
        int ColorG { get; set; }
        int ColorB { get; set; }
        public Color Color
        {
            get { return Color.FromArgb(ColorR, ColorG, ColorB); }
            set
            {
                ColorR = value.R;
                ColorG = value.G;
                ColorB = value.B;
            }
        }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; } = null!;
    }
}
