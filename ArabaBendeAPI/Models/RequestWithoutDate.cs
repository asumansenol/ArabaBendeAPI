using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabaBendeAPI.Models
{
    public class RequestWithoutDate
    {
        public int RequestID { get; set; }
        public int PersonID { get; set; }
        public Nullable<int> DurationCheck { get; set; }
        public int VehicleID { get; set; }
    }
}