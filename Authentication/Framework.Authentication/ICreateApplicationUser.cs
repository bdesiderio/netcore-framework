using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication
{
    public interface ICreateApplicationUserRequest
    {
        string Username { get; }
        string Password { get; }
        IEnumerable<string> UserRoles { get; }
    }
}
