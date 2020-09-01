using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Authentication.Identity
{
    public abstract class IdentityAuthenticationManager<TApplicationUser, TApplicationUserKey, TIdentityRole, TIdentityRoleKey> : IAuthenticationManager
        where TApplicationUser : IdentityUser<TApplicationUserKey>, IApplicationUser
        where TIdentityRole : IdentityRole<TIdentityRoleKey>
        where TIdentityRoleKey : IEquatable<TIdentityRoleKey>
        where TApplicationUserKey : IEquatable<TApplicationUserKey>
    {
        protected readonly UserManager<TApplicationUser> UserManager;
        protected readonly RoleManager<IdentityRole<TIdentityRoleKey>> RoleManager;

        public IdentityAuthenticationManager(UserManager<TApplicationUser> userManager,
             RoleManager<IdentityRole<TIdentityRoleKey>> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public async Task<IApplicationUser> CheckLoginAndGetUser(string username, string password)
        {
            var user = await UserManager.FindByNameAsync(username);

            if (user != null && await UserManager.CheckPasswordAsync(user, password))
            {
                return user;
            }

            return null;
        }

        protected abstract TApplicationUser CreateApplicationUserInstance();
        protected virtual Task<TApplicationUser> MapApplicationUserCustomData(TApplicationUser applicationUser, ICreateApplicationUserRequest applicationUserRequest) =>
            Task.FromResult(applicationUser);

        public async Task<IApplicationUser> CreateUser(ICreateApplicationUserRequest applicationUserRequest)
        {
            TApplicationUser user = CreateApplicationUserInstance();

            user.SecurityStamp = Guid.NewGuid().ToString();
            user.UserName = applicationUserRequest.Username;

            await MapApplicationUserCustomData(user, applicationUserRequest);

            var result = await UserManager.CreateAsync(user, applicationUserRequest.Password);

            if (!result.Succeeded)
            {
                return null;
            }

            var tasks = applicationUserRequest.UserRoles.Select(async userRole =>
            {
                if (!await RoleManager.RoleExistsAsync(userRole))
                {
                    await RoleManager.CreateAsync(new IdentityRole<TIdentityRoleKey>(userRole));
                }

                if (await RoleManager.RoleExistsAsync(userRole))
                {
                    await UserManager.AddToRoleAsync(user, userRole);
                }
            });

            await Task.WhenAll(tasks);

            return await GetUserAsync(applicationUserRequest.Username);
        }

        public async Task<List<string>> GetRolesAsync(IApplicationUser username)
        {
            return (await UserManager.GetRolesAsync(username as TApplicationUser)).ToList();
        }

        public async Task<IApplicationUser> GetUserAsync(string username)
        {
            return await UserManager.FindByNameAsync(username);
        }

        public async Task DeleteUser(IApplicationUser applicationUser)
        {
            await UserManager.DeleteAsync(applicationUser as TApplicationUser);
        }

        public async Task ChangePassword(IApplicationUser applicationUser, string currentPassword, string newPassword)
        {
            await UserManager.ChangePasswordAsync(applicationUser as TApplicationUser, currentPassword, newPassword);
        }
    }
}
