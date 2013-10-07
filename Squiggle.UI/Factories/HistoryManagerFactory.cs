using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Squiggle.History;

namespace Squiggle.UI.Factories
{
    class HistoryManagerFactory: IInstanceFactory<HistoryManager>
    {
        public HistoryManager CreateInstance()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["HistoryContext"];
            if (setting == null)
                return null;

            string connectionString = Environment.ExpandEnvironmentVariables(setting.ConnectionString);
            return new HistoryManager(connectionString);
        }
    }
}
