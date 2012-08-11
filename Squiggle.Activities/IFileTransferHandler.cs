using System;
using System.ComponentModel;
using System.IO;
using Squiggle.Core.Chat;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.Activities
{
    public interface IFileTransferHandler: IActivityHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
