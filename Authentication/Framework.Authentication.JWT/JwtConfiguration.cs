using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT
{
    public class JwtConfiguration
    {
        public string JwtSecret { get; set; }
        public string JwtValidUser { get; set; }
        public string JwtValidAudience { get; set; }
    }
}
