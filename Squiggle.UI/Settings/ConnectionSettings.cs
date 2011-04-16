
using Squiggle.Chat.Services;
namespace Squiggle.UI.Settings
{
    class ConnectionSettings
    {
        public int PresencePort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public string BindToIP { get; set; }
        public string PresenceAddress { get; set; }
        public string ClientID { get; set; }

        public ConnectionSettings()
        {
            PresencePort = DefaultValues.PresencePort;
            ChatPort = DefaultValues.ChatPort;
            KeepAliveTime = DefaultValues.KeepAliveTime;
            PresenceAddress = DefaultValues.PresenceAddress;
        }
    }
}
