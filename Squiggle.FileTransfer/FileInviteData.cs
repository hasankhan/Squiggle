using System;
using System.Collections.Generic;
using System.Linq;

namespace Squiggle.FileTransfer
{
    /// <summary>
    /// Serializes/deserializes file transfer invite metadata (name and size)
    /// exchanged during the gRPC activity invitation handshake.
    /// </summary>
    class FileInviteData : IEnumerable<KeyValuePair<string, string>>
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }

        public FileInviteData() { }

        public FileInviteData(IEnumerable<KeyValuePair<string, string>> data)
        {
            var dictionary = data.ToDictionary(i => i.Key, i => i.Value);

            if (dictionary.TryGetValue("name", out var name))
                Name = name;

            if (dictionary.TryGetValue("size", out var sizeStr) && long.TryParse(sizeStr, out var size))
                Size = size;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            yield return new KeyValuePair<string, string>("name", Name);
            yield return new KeyValuePair<string, string>("size", Size.ToString());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
