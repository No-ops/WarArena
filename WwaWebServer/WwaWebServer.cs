using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WwaWebServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    class WwaWebServer : IWwaWebServer
    {
        public Statistics GetStats()
        {
            return new Statistics();
        }
    }
}
