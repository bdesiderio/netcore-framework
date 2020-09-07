using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface IApplicationUser<TKey>
    {
        public TKey Id { get; }
        public string UserName { get; set; }
    }
}
