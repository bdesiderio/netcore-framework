using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication
{
    public interface IAuthenticationManager<TKey>
    {
        Task<IApplicationUser<TKey>> GetUserAsync(string username);
        Task<IApplicationUser<TKey>> CheckLoginAndGetUser(string username, string password);
        Task<List<string>> GetRolesAsync(IApplicationUser<TKey> username);
        Task<IApplicationUser<TKey>> CreateUser(ICreateApplicationUserRequest applicationUserRequest);
        Task DeleteUser(IApplicationUser<TKey> applicationUser);
        Task ChangePassword(IApplicationUser<TKey> applicationUser, string oldPassword, string newPassword);
    }
}
