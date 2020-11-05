using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignallingServer.Models
{
    public class Answer
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Sdp { get; set; }
        public string Type { get; set; }
        public bool IsBusy { get; set; }
        public bool IsRejected { get; set; }
    }
}
