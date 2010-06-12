using System.Configuration;

namespace Squiggle.Bridge.Configuration
{
    class TargetCollection : ConfigurationElementCollection
    {
        public Target this[int index]
        {
            get
            {
                return base.BaseGet(index) as Target;
            }
            set
            {
                if (base.BaseGet(index) != null)
                    base.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Target();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Target)element).IP;
        }

    }
}
