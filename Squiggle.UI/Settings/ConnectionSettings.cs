
using Squiggle.Core;
namespace Squiggle.UI.Settings
{
    class ConnectionSettings
    {
        public int PresencePort { get; set; }
        public int PresenceCallbackPort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public string BindToIP { get; set; }
        public string PresenceAddress { get; set; }
        public string ClientID { get; set; }
    }
}
