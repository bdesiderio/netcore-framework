using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Authentication.ServiceContracts
{
    public interface ISessionPersistanceService<TApplicationUserKey>
    {
        Task<IPersistSessionClientResponse> PersistSession(IApplicationUser<TApplicationUserKey> applicationUser, IEnumerable<string> roles);
        Task<IUserSession<TApplicationUserKey>> GetSessionPersistance();
    }
}
