using System.Configuration;

namespace Squiggle.Bridge.Configuration
{
    class BridgeConfiguration : ConfigurationSection
    {
        public static BridgeConfiguration GetConfig()
        {
            return (BridgeConfiguration)ConfigurationManager.GetSection("BridgeConfiguration") ?? new BridgeConfiguration();
        }

        [ConfigurationProperty("ExternalServiceBinding")]
        public Target ExternalServiceBinding
        {
            get { return (Target)this["ExternalServiceBinding"] ?? new Target(); }
        }

        [ConfigurationProperty("InternalServiceBinding")]
        public Target InternalServiceBinding
        {
            get { return (Target)this["InternalServiceBinding"] ?? new Target(); }
        }

        [ConfigurationProperty("PresenceBinding")]
        public PresenceBinding PresenceBinding
        {
            get { return (PresenceBinding)this["PresenceBinding"] ?? new PresenceBinding(); }
        }

        [ConfigurationProperty("Targets")]
        public TargetCollection Targets
        {
            get
            {
                return (TargetCollection)this["Targets"] ??
                   new TargetCollection();
            }
        }

    }
}
