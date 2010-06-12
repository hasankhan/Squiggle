using System.Configuration;

namespace Squiggle.Bridge.Configuration
{
    class BridgeConfiguration : ConfigurationSection
    {
        public static BridgeConfiguration GetConfig()
        {
            return (BridgeConfiguration)ConfigurationManager.GetSection("BridgeConfiguration") ?? new BridgeConfiguration();
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
