using System;
using Squiggle.Client;
using System.Collections;
using System.Collections.Generic;
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
        void SendFiles(IEnumerable<string> filePaths);
        void SendMessage(string message);
    }
}
