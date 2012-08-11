using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Activities
{
    public interface IActivityExecutor
    {
        bool SelfCancelled { get; }
        bool IsConnected { get; }
        bool SelfInitiated { get; }
        long BytesReceived { get; }

        void Start();
        void Cancel();
        void SetHandler(ActivityHandler activityHandler);
        void SendData(byte[] chunk);
        void UpdateProgress(int percentage);
        void CompleteTransfer();
        void Accept();
    }
}
