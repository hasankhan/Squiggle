using System;
using System.ComponentModel;
using System.IO;
using Squiggle.Core.Chat;

namespace Squiggle.Activities
{
    public interface IFileTransfer: IAppHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
