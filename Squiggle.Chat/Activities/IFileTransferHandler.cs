using System;
using System.ComponentModel;
using System.IO;

namespace Squiggle.Activities
{
    public interface IFileTransferHandler: IActivityHandler
    {
        long Size { get; }
        string Name { get; }

        void Accept(string filePath);
    }
}
