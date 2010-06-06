using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Squiggle.UI
{
    public class SettingBinding : Binding
    {
        public SettingBinding(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = Squiggle.UI.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
