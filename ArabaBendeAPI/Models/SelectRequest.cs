using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabaBendeAPI.Models
{
    public class SelectRequest
    {
        public string isim { get; set; }
        public string TelephoneNumber { get; set; }
        public string Plaka { get; set; }
        public Nullable<int> DurationCheck { get; set; }
        public string PhotoUrl { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
    }
}