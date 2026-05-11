using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Squiggle.Utilities;

namespace Squiggle.UI.Components
{
    record UpdateCheckResult
    {
        public bool IsUpdated { get; init; }
        public DateTime LastUpdate { get; init; }
        public string Title { get; init; } = null!;
        public string UpdateLink { get; init; } = null!;
    }

    class UpdateNotifier
    {
        const string versionRegex = "Squiggle (?<version>\\d(?:\\.\\d+)+)";

        public static UpdateCheckResult CheckForUpdate(DateTimeOffset clientLastUpdate)
        {
            SyndicationItem? lastUpdate = GetLastUpdate();
            if (lastUpdate != null && lastUpdate.PublishDate > clientLastUpdate && VersionIsSameOrNewer(lastUpdate))
            {
                return new UpdateCheckResult
                {
                    IsUpdated = true,
                    LastUpdate = lastUpdate.PublishDate.LocalDateTime,
                    Title = lastUpdate.Title.Text,
                    UpdateLink = lastUpdate.Links.FirstOrDefault().Uri.ToString()
                };
            }

            return new UpdateCheckResult();
        }

        static bool VersionIsSameOrNewer(SyndicationItem lastUpdate)
        {
            if (!Regex.IsMatch(lastUpdate.Title.Text, versionRegex))
                return false;

            string version = Regex.Match(lastUpdate.Title.Text, versionRegex).Groups["version"].Value;

            bool sameOrNewer = new Version(version) >= new Version(AppInfo.Version.Major, AppInfo.Version.Minor);
            return sameOrNewer;
        }

        static SyndicationItem? GetLastUpdate()
        {
            var feed = SyndicationFeed.Load(new XmlTextReader("http://squiggle.codeplex.com/project/feeds/rss?ProjectRSSFeed=codeplex%3a%2f%2frelease%2fsquiggle"));
            SyndicationItem? lastUpdate = feed.Items.OrderByDescending(item => item.PublishDate).FirstOrDefault();
            return lastUpdate;
        }
    }
}
