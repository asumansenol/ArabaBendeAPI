using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabaBendeAPI.Models
{
    public class ReservationStringDatetime
    {
        public int ReservationID { get; set; }
        public int VehicleID { get; set; }
        public string Aim { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public int PersonID { get; set; }

    }
}