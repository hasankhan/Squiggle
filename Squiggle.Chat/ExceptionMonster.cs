using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Squiggle.Chat
{
    public static class ExceptionMonster
    {
        public static void EatTheException(Action action, string actionDescription)
        {
            EatTheException(() =>
            {
                action();
                return (object)null;
            }, actionDescription);
        }

        public static T EatTheException<T>(Func<T> action, string actionDescription)
        {
            bool success;
            return EatTheException(action, actionDescription, out success);
        }

        // Oh well I know this is ugly but bridge has got to stay alive right?
        public static T EatTheException<T>(Func<T> action, string actionDescription, out bool success)
        {
            try
            {                
                T result = action();
                success = true;
                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Erorr occured while " + actionDescription + ": " + ex.Message);
            }
            success = false;
            return default(T);
        }
    }
}
