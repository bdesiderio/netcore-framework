using Framework.Authentication;
using Framework.Authorization.Policies;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Authorization
{
    public interface IAuthorizationService<TUser, TRole>
    {
        Task AuthorizeAsync(TUser user, RoleBasedAuthorizationPolicy<TRole> policy, Func<Task> func);
        Task<TResult> AuthorizeAsync<TResult>(TUser user, RoleBasedAuthorizationPolicy<TRole> policy, Func<Task<TResult>> func);
        Task AuthorizeAsync<TPolicy, TResource>(TUser user, TResource resource, Func<Task> func) where TPolicy : ResourceBasedAuthorizationPolicy<TUser, TResource>;
        Task<TResult> AuthorizeAsync<TPolicy, TResource, TResult>(TUser user, TResource resource, Func<Task<TResult>> func) where TPolicy : ResourceBasedAuthorizationPolicy<TUser, TResource>;
    }

    public abstract class AuthorizationService<TUser, TRole> : IAuthorizationService<TUser, TRole>
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AuthorizeAsync(TUser user, RoleBasedAuthorizationPolicy<TRole> policy, Func<Task> func)
        {
            if (!await CheckAuthorizationAsync(user, policy))
            {
                throw new UnauthorizedAccessException();
            }

            await func();
        }

        public async Task<TResult> AuthorizeAsync<TResult>(TUser user, RoleBasedAuthorizationPolicy<TRole> policy, Func<Task<TResult>> func)
        {
            if (!await CheckAuthorizationAsync(user, policy))
            {
                throw new UnauthorizedAccessException();
            }

            return await func();
        }

        public async Task AuthorizeAsync<TPolicy, TResource>(TUser user, TResource resource, Func<Task> func)
            where TPolicy : ResourceBasedAuthorizationPolicy<TUser, TResource>
        {
            if (!await CheckAuthorizationAsync<TPolicy, TResource>(user, resource))
            {
                throw new UnauthorizedAccessException();
            }

            await func();
        }

        public async Task<TResult> AuthorizeAsync<TPolicy, TResource, TResult>(TUser user, TResource resource, Func<Task<TResult>> func)
            where TPolicy : ResourceBasedAuthorizationPolicy<TUser, TResource>
        {
            if (!await CheckAuthorizationAsync<TPolicy, TResource>(user, resource))
            {
                throw new UnauthorizedAccessException();
            }

            return await func();
        }

        protected abstract Task<bool> CheckAuthorizationAsync(TUser user, RoleBasedAuthorizationPolicy<TRole> policy);

        protected virtual async Task<bool> CheckAuthorizationAsync<TPolicy, TResource>(TUser user, TResource resource)
            where TPolicy : ResourceBasedAuthorizationPolicy<TUser, TResource>
        {
            var policy = _serviceProvider.GetRequiredService<TPolicy>();
            return await policy.CheckAsync(user, resource);
        }
    }
}
