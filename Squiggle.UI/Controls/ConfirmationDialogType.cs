using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Resources;

namespace Squiggle.UI.Controls
{
    public class ConfirmationDialogType
    {
        public int Code {get; private set; }
        public string Title {get; private set; }
        public string Message {get; private set; }

        public static ConfirmationDialogType FileTransferWindowClose
        {
            get
            {
                return new ConfirmationDialogType()
                { 
                    Code = 1, 
                    Title = Translation.Instance.Confirmation_FileTransferWindowClose_Title, 
                    Message = Translation.Instance.Confirmation_FileTransferWindowClose_Message
                };
            }
        }
    }
}
