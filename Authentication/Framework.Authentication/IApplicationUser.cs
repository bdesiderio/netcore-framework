using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface IApplicationUser
    {
        public object Id { get; }
        public string UserName { get; set; }
    }
}
