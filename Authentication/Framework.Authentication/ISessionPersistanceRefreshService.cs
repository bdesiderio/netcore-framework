using Framework.Authentication.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication
{
    public interface ISessionPersistanceRefreshService<TApplicationUserKey> : ISessionPersistanceService<TApplicationUserKey>
    {
        Task<IPersistSessionClientResponse> RefreshSession();
    }
}
