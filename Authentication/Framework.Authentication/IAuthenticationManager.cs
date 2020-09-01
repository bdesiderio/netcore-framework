using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication
{
    public interface IAuthenticationManager
    {
        Task<IApplicationUser> GetUserAsync(string username);
        Task<IApplicationUser> CheckLoginAndGetUser(string username, string password);
        Task<List<string>> GetRolesAsync(IApplicationUser username);
        Task<IApplicationUser> CreateUser(ICreateApplicationUserRequest applicationUserRequest);
        Task DeleteUser(IApplicationUser applicationUser);
        Task ChangePassword(IApplicationUser applicationUser, string oldPassword, string newPassword);
    }
}
