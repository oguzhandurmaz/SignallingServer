using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignallingServer.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set;}
        public string RoomName { get; set; }
        public bool IsBusy { get; set; }
    }
}
