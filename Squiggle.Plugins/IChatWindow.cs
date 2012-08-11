using System;
namespace Squiggle.Plugins
{
    public interface IChatWindow
    {
        System.Collections.Generic.IEnumerable<Squiggle.Chat.Buddy> Buddies { get; }
        void Invite(Squiggle.Chat.Buddy buddy);
        void Invite(System.Collections.Generic.IEnumerable<Squiggle.Chat.Buddy> buddies);
        bool IsGroupChat { get; }
        Squiggle.Chat.Buddy PrimaryBuddy { get; }
        void Restore();
        void SaveTo(string fileName, string format);
        void SendBuzz();
        void SendFile();
        void SendFiles(params string[] filePaths);
        void SendMessage(string message);
    }
}
