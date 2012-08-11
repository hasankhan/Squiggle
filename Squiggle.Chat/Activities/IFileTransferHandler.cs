using System;
using System.ComponentModel;
using System.IO;
using Squiggle.Activities;

namespace Squiggle.Chat.Activities
{
    public interface IFileTransferHandler: IActivityHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
