using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Authentication.ServiceContracts
{
    public interface ISessionPersistanceService<TApplicationUserKey>
    {
        Task<IPersistSessionClientResponse> PersistSession(IApplicationUser<TApplicationUserKey> applicationUser, IEnumerable<string> roles, bool persist);
        Task LeaveSession();
        Task<IUserSession<TApplicationUserKey>> GetUserSession();
    }
}
