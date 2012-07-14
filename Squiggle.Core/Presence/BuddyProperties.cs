using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Squiggle.Core.Presence
{
    public class BuddyProperties: INotifyPropertyChanged
    {
        Dictionary<string, string> dictionary;

        public static readonly string DefaultGroupName = "Others";
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public BuddyProperties(Dictionary<string, string> properties)
        {
            this.dictionary = properties;
        }

        public BuddyProperties()
        {
            this.dictionary = new Dictionary<string, string>();
        }

        public string this[string key]
        {
            get
            {
                string value;
                dictionary.TryGetValue(key, out value);
                return value;
            }
            set
            {
                dictionary[key] = value;
                PropertyChanged(this, new PropertyChangedEventArgs(key));
            }
        }

        public string GroupName
        {
            get 
            { 
                string groupName = this["GroupName"];
                return String.IsNullOrEmpty(groupName) ? DefaultGroupName : groupName;
            }
            set { this["GroupName"] = value.Trim(); }
        }

        public string MachineName
        {
            get { return this["MachineName"]; }
            set { this["MachineName"] = value; }
        }

        public string DisplayMessage
        {
            get { return this["DisplayMessage"]; }
            set { this["DisplayMessage"] = value.Trim(); }
        }

        public byte[] DisplayImage
        {
            get 
            { 
                string image = this["DisplayImage"];
                if (String.IsNullOrEmpty(image))
                    return null;
                else
                    try
                    {
                        return Convert.FromBase64String(image);
                    }
                    catch (FormatException)
                    {
                        return null;
                    }
            }
            set 
            {
                string image = null;
                if (value != null)
                    image = Convert.ToBase64String(value);
                this["DisplayImage"] = image; 
            }
        }

        public string EmailAddress
        {
            get { return this["EmailAddress"]; }
            set { this["EmailAddress"] = value; }
        }

        public Dictionary<string,string>.KeyCollection Keys
        {
            get { return dictionary.Keys; }
        }

        public Dictionary<string, string>.ValueCollection Values
        {
            get { return dictionary.Values; }
        }

        public BuddyProperties Clone()
        {
            return new BuddyProperties(ToDictionary());
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(dictionary);
        }
    }
}
