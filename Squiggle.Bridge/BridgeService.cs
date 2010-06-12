using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Squiggle.Bridge
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class BridgeService
    {
    }
}
