using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT
{
    public class JwtSessionClientResponse : IPersistSessionClientResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
