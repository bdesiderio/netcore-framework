using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface IApplicationUser
    {
        public string UserName { get; set; }
    }
}
