using Squiggle.Core;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;
using System;

namespace Squiggle.UI.Settings
{
    class ConnectionSettings : ISettingsGroup
    {
        public int PresencePort { get; set; }
        public int PresenceCallbackPort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public string BindToIP { get; set; }
        public string PresenceAddress { get; set; }
        public string ClientID { get; set; }

        public void ReadFrom(Properties.Settings settings, ConfigReader reader)
        {
            PresenceAddress = reader.GetSetting(SettingKey.PresenceAddress, DefaultValues.PresenceAddress);
            BindToIP = settings.BindToIP;

            bool requiresNewBindToIP = !NetworkUtility.IsValidLocalIP(BindToIP);
            if (requiresNewBindToIP)
            {
                var ip = NetworkUtility.GetLocalIPAddress();
                BindToIP = ip == null ? String.Empty : ip.ToString();
            }

            ClientID = settings.ClientID;

            if (String.IsNullOrEmpty(ClientID))
                ClientID = Guid.NewGuid().ToString();

            ChatPort = reader.GetSetting(SettingKey.ChatPort, DefaultValues.ChatPort);
            KeepAliveTime = reader.GetSetting(SettingKey.KeepAliveTime, DefaultValues.KeepAliveTime);
            PresencePort = reader.GetSetting(SettingKey.PresencePort, DefaultValues.PresencePort);
            PresenceCallbackPort = reader.GetSetting(SettingKey.PresenceCallbackPort, DefaultValues.PresenceCallbackPort);
        }

        public void WriteTo(Properties.Settings settings, ConfigReader reader)
        {
            reader.SetSetting(SettingKey.PresenceAddress, PresenceAddress);
            settings.BindToIP = BindToIP;
            reader.SetSetting(SettingKey.ChatPort, ChatPort);
            reader.SetSetting(SettingKey.KeepAliveTime, KeepAliveTime);
            reader.SetSetting(SettingKey.PresencePort, PresencePort);
            reader.SetSetting(SettingKey.PresenceCallbackPort, PresenceCallbackPort);
            settings.ClientID = ClientID;
        }
    }
}
