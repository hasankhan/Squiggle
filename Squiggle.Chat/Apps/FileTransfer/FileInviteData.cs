using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.Apps.FileTransfer
{
    class FileInviteData : IEnumerable<KeyValuePair<string, string>>
    {
        public string Name { get; set; }
        public long Size { get; set; }

        public FileInviteData() { }
        public FileInviteData(IEnumerable<KeyValuePair<string, string>> data)
        {
            var dictionary = data.ToDictionary(i => i.Key, i => i.Value);
            string temp;
            dictionary.TryGetValue("name", out temp);
            Name = temp;
            dictionary.TryGetValue("size", out temp);
            long size = 0;
            if (!String.IsNullOrEmpty(temp))
                Int64.TryParse(temp, out size);
            Size = size;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            yield return new KeyValuePair<string, string>("name", Name);
            yield return new KeyValuePair<string, string>("size", Size.ToString());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
