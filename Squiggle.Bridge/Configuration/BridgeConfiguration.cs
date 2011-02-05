using System.Configuration;

namespace Squiggle.Bridge.Configuration
{
    class BridgeConfiguration : ConfigurationSection
    {
        public static BridgeConfiguration GetConfig()
        {
            return (BridgeConfiguration)ConfigurationManager.GetSection("BridgeConfiguration") ?? new BridgeConfiguration();
        }

        [ConfigurationProperty("externalservicebinding")]
        public Target RemoteServiceBinding
        {
            get { return (Target)this["externalservicebinding"] ?? new Target(); }
        }

        [ConfigurationProperty("internalservicebinding")]
        public Target LocalServiceBinding
        {
            get { return (Target)this["internalservicebinding"] ?? new Target(); }
        }

        [ConfigurationProperty("channelbinding")]
        public ChannelBinding ChannelBinding
        {
            get { return (ChannelBinding)this["channelbinding"] ?? new ChannelBinding(); }
        }

        [ConfigurationProperty("targets")]
        public TargetCollection Targets
        {
            get
            {
                return (TargetCollection)this["targets"] ??
                   new TargetCollection();
            }
        }

    }
}
