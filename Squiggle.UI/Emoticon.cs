using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI
{
    class Emoticon
    {
        public Uri ImageUri { get; private set; }
        public string Title { get; private set; }
        public string[] Codes { get; private set; }

        public Emoticon(Uri imageUri, string title, params string[] codes)
        {
            this.ImageUri = imageUri;
            this.Title = title;
            this.Codes = codes;
        }
    }
}
