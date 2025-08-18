using System.Collections.Generic;
using System.Threading.Tasks;
using UMA.Models;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Models.Entity;

namespace UMA.Services
{
    public interface IUserService
    {
        Task<UserRequest> GetUserByEmail(string email);
        Task<bool> AddUserAsync(UserRequest userRequest);
        Task<bool> UpdateUserAsync(UserRequest userRequest);
        Task<bool> DeleteUserAsync(int id);
        Task<UserResponse> ValidateUser(LoginRequest loginRequest);
    }
}