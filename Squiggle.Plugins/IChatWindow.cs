using System;
using Squiggle.Chat;
namespace Squiggle.Plugins
{
    public interface IChatWindow
    {
        System.Collections.Generic.IEnumerable<IBuddy> Buddies { get; }
        void Invite(IBuddy buddy);
        void Invite(System.Collections.Generic.IEnumerable<IBuddy> buddies);
        bool IsGroupChat { get; }
        IBuddy PrimaryBuddy { get; }
        void Restore();
        void SaveTo(string fileName, string format);
        void SendBuzz();
        void SendFile();
        void SendFiles(params string[] filePaths);
        void SendMessage(string message);
    }
}
