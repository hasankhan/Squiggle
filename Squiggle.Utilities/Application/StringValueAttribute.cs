using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Squiggle.Utilities.Application
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute: Attribute
    {
        public string Value { get; set; }

        public StringValueAttribute(string value)
        {
            Value = value;
        }

        public static string GetValue(object item)
        {
            MemberInfo member = item.GetType().GetMember(item.ToString()).FirstOrDefault();
            StringValueAttribute attribute = member.GetCustomAttributes(typeof(StringValueAttribute), false).Cast<StringValueAttribute>().FirstOrDefault();
            return attribute.Value;
        }
    }
}
