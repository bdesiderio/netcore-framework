using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT
{
    public class JwtConfiguration : IOptions<JwtConfiguration>
    {
        public JwtConfiguration Value => this;

        public string JwtSecret { get; set; }
        public string JwtValidUser { get; set; }
        public string JwtValidAudience { get; set; }
        public int TokenExpirationSeconds { get; set; } = 10800;
    }
}
