using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Authentication.ServiceContracts
{
    public interface ISessionPersistanceService
    {
        Task<object> PersistSession(IApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
