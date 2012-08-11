using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Squiggle.UI.Components;
using Squiggle.Activities;
using Squiggle.Plugins.Activity;
using System.Windows.Input;

namespace Squiggle.UI.Helpers
{
    class ActivitiesMenuHelper
    {
        SquiggleContext context;

        public ActivitiesMenuHelper(SquiggleContext context)
        {
            this.context = context;
        }

        public void LoadActivities(MenuItem mnuStartActivity, MenuItem mnuNoActivity, ICommand handler)
        {
            var activities = context.PluginLoader.Activities.Where(a => !SquiggleActivities.All.Contains(a.Id));
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
