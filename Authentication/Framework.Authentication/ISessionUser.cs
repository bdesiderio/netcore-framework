using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface IUserSession
    {
        public object Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }

    public class SessionUser : IUserSession
    {
        public object Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
