using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MamRenewer.Services
{
    class PreviousJobInfoRepository
    {
        //Store in memory for now, no need to use sqlite yet
        private static string _lastUsedIP = null;

        public string RetrieveLastUsedIP()
        {
            return _lastUsedIP;
        }
        public void UpdateLastUsedIP(string ip)
        {
            _lastUsedIP = ip;
        }
    }
}
