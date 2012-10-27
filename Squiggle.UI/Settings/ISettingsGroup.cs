using Squiggle.Utilities.Application;
using System;
namespace Squiggle.UI.Settings
{
    interface ISettingsGroup
    {
        void ReadFrom(Squiggle.UI.Properties.Settings settings, ConfigReader reader);
        void WriteTo(Squiggle.UI.Properties.Settings settings, ConfigReader reader);
    }
}
