using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface IUserSession<TKey>
    {
        public TKey Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }

    public class SessionUser<TKey> : IUserSession<TKey>
    {
        public TKey Id { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
