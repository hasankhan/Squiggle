using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Squiggle.UI.Components;
using Squiggle.Core.Chat.Activity;
using System.Windows.Input;
using Squiggle.Client.Activities;
using Squiggle.Client;

namespace Squiggle.UI.Helpers
{
    class ActivityHelper
    {
        SquiggleContext context;

        public ActivityHelper(SquiggleContext context)
        {
            this.context = context;
        }

        public void LoadActivitiesMenu(MenuItem mnuStartActivity, MenuItem mnuNoActivity, ICommand handler)
        {
            var activities = from activity in context.PluginLoader.Activities
                             let isBuiltIn = SquiggleActivities.All.Contains(activity.Id)
                             let isDerivedActivity = typeof(IActivity).IsAssignableFrom(activity.GetType().BaseType)
                             where !isBuiltIn || isDerivedActivity
                             select activity;

            foreach (IActivity activity in activities)
            {
                var item = new MenuItem();
                item.Header = activity.Title;
                item.CommandParameter = activity;
                item.Command = handler;
                mnuStartActivity.Items.Add(item);
            }

            mnuNoActivity.Visibility = activities.Any() ? Visibility.Collapsed : System.Windows.Visibility.Visible;            
        }
    }
}
