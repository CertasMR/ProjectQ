using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectQ.Models
{
    public class HotNearbyModel
    {
        public string YourTown { get; set; }
        public decimal YourTemp { get; set; }
        public string HotTown { get; set; }
        public decimal HotTemp { get; set; }
        public decimal Distance { get; set; }
        public int MinsToHottest { get; set; }
    }
}