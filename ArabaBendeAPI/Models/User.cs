using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabaBendeAPI.Models
{
    public class User
    {
        public string id { get; set; }
        public string familyName { get; set; }
        public string givenName { get; set; }
        public string photoUrl { get; set; }
        public string email { get; set; }
        public string name { get; set; }
    }
}