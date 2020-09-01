using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Authentication.ServiceContracts
{
    public interface ISessionPersistanceService
    {
        Task<IPersistSessionClientResponse> PersistSession(IApplicationUser applicationUser, IEnumerable<string> roles);
        Task<IUserSession> GetSessionPersistance();
    }
}
