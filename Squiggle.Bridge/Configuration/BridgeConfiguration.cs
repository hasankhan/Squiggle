using System.Configuration;

namespace Squiggle.Bridge.Configuration
{
    class BridgeConfiguration : ConfigurationSection
    {
        public static BridgeConfiguration GetConfig()
        {
            return (BridgeConfiguration)ConfigurationManager.GetSection("BridgeConfiguration") ?? new BridgeConfiguration();
        }

        [ConfigurationProperty("servicebinding")]
        public Target ServiceBinding
        {
            get { return (Target)this["servicebinding"] ?? new Target(); }
        }

        [ConfigurationProperty("channelbinding")]
        public Target ChannelBinding
        {
            get { return (Target)this["channelbinding"] ?? new Target(); }
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
