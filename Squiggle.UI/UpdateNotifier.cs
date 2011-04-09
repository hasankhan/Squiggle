using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Reflection;
using System.IO;

namespace Squiggle.UI
{
    class UpdateCheckResult
    {
        public bool IsUpdated { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Title { get; set; }
        public string UpdateLink { get; set; }
    }

    class UpdateNotifier
    {
        public static void CheckForUpdate(Action<UpdateCheckResult> onUpdateCheckComplete)
        {
            Async.Invoke(() =>
            {
                var result = new UpdateCheckResult();

                var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                if (fileInfo.Exists)
                {
                    DateTime fileTime = fileInfo.CreationTimeUtc;
                    SyndicationItem lastUpdate = GetLastUpdate();
                    if (lastUpdate != null && lastUpdate.PublishDate.UtcDateTime>fileTime)
                    {
                        result.LastUpdate = lastUpdate.PublishDate.LocalDateTime;
                        result.IsUpdated = true;
                        result.Title = lastUpdate.Title.Text;
                        result.UpdateLink = lastUpdate.Links.FirstOrDefault().Uri.ToString();
                    }
                }

                onUpdateCheckComplete(result);
            });
        }

        static SyndicationItem GetLastUpdate()
        {
            var feed = SyndicationFeed.Load(new XmlTextReader("http://squiggle.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2frelease%2fsquiggle"));
            SyndicationItem lastUpdate = feed.Items.OrderByDescending(item => item.PublishDate).FirstOrDefault();
            return lastUpdate;
        }
    }
}
