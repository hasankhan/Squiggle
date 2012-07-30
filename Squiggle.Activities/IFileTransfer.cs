using System;
using System.ComponentModel;
using System.IO;
using Squiggle.Core.Chat;

namespace Squiggle.Activities
{
    public interface IFileTransfer: IActivityHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
