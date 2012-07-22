using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Squiggle.Utilities
{
    public abstract class SerializationSurrogate<T>
    {
        public SerializationSurrogate(T obj)
        {
            string propName = obj.GetType().Name;
            PropertyInfo propInfo = this.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic);
            propInfo.SetValue(this, obj, null);
        }

        public T GetObject()
        {
            var properties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            var message = (T)properties.Select(propInfo => propInfo.GetValue(this, null)).Where(x => x != null).FirstOrDefault();
            return message;
        }
    }
}
