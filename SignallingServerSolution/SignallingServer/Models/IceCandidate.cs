using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignallingServer.Models
{
    public class IceCandidate
    {
        public string To { get; set; }
        public string Type { get; set; }
        public string Candidate { get; set; }
    }
}
