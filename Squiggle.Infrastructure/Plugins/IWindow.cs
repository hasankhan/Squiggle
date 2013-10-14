using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins
{
    public interface IWindow
    {
        void Hide();
        void Restore();
    }
}
