using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using System.IO;
using System.Drawing;

namespace Squiggle.Chat.Services
{
    [ServiceContract]
    public interface IChatHost
    {
        [OperationContract]
        void UserIsTyping(IPEndPoint user);

        [OperationContract]
        void ReceiveMessage(IPEndPoint user, string fontName, int fontSize, Color color, string message);

        [OperationContract]
        void ReceiveFileInvite(IPEndPoint user, Guid id, string name, int size);

        [OperationContract]
        void ReceiveFileContent(Guid id, byte[] chunk);

        [OperationContract]
        void AcceptFileInvite(Guid id);

        [OperationContract]
        void CancelFileTransfer(Guid id);
    }
}
