using System;
using System.ComponentModel;
using System.IO;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat
{
    public interface IFileTransfer: IAppHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
