using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authorization.Policies
{
    public abstract class ResourceBasedAuthorizationPolicy<TUser, TResource>
    {
        public abstract Task<bool> CheckAsync(TUser user, TResource resource);
    }
}
