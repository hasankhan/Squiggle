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
