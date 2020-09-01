using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authorization.Policies
{
    public static class RolePolicy
    {
        public static RoleBasedAuthorizationPolicy<TRole> From<TRole>(params TRole[] roles)
            => new RoleBasedAuthorizationPolicy<TRole>(roles);
    }

    public class RoleBasedAuthorizationPolicy<TRole>
    {
        public RoleBasedAuthorizationPolicy(IEnumerable<TRole> roles)
        {
            Roles = new HashSet<TRole>(roles);
        }

        public HashSet<TRole> Roles { get; }
    }
}
