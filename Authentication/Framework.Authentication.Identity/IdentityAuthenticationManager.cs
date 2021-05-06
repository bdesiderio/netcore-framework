using Framework.Authentication.Exceptions;
using Framework.Authentication.Exceptions.ChangePasswordExceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Authentication.Identity
{
    public abstract class IdentityAuthenticationManager<TApplicationUser, TApplicationUserKey, TIdentityRole, TIdentityRoleKey> : IAuthenticationManager<TApplicationUserKey>
        where TApplicationUser : IdentityUser<TApplicationUserKey>, IApplicationUser<TApplicationUserKey>
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

        public async Task<IApplicationUser<TApplicationUserKey>> CheckLoginAndGetUser(string username, string password)
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

        public async Task<IApplicationUser<TApplicationUserKey>> CreateUser(ICreateApplicationUserRequest applicationUserRequest)
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

        public async Task<List<string>> GetRolesAsync(IApplicationUser<TApplicationUserKey> username)
        {
            return (await UserManager.GetRolesAsync(username as TApplicationUser)).ToList();
        }

        public async Task<IApplicationUser<TApplicationUserKey>> GetUserAsync(string username)
        {
            return await UserManager.FindByNameAsync(username);
        }

        public async Task DeleteUser(IApplicationUser<TApplicationUserKey> applicationUser)
        {
            await UserManager.DeleteAsync(applicationUser as TApplicationUser);
        }

        public async Task ChangePassword(IApplicationUser<TApplicationUserKey> applicationUser, string currentPassword, string newPassword)
        {
            var result = await UserManager.ChangePasswordAsync(applicationUser as TApplicationUser, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(x => x.Code == "PasswordRequiresDigit"))
                    throw new PasswordLowSecurityException(String.Join(Environment.NewLine, result.Errors.Select(x => x.Description)));
                if (result.Errors.Any(x => x.Code == "PasswordMismatch"))
                    throw new PasswordMisMatchException(String.Join(Environment.NewLine, result.Errors.Select(x => x.Description)));
                throw new ChangePasswordException(String.Join(Environment.NewLine, result.Errors.Select(x => x.Description)));
            }
        }
    }
}
