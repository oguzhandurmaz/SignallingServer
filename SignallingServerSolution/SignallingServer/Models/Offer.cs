using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignallingServer.Models
{
    public class Offer
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Sdp { get; set; }
        public string Type { get; set; }
    }
}
