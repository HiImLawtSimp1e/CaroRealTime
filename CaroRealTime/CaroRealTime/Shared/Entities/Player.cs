using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaroRealTime.Shared.Entities
{
    public class Player
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Player(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }
    }
}
