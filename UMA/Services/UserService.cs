using Microsoft.AspNetCore.Identity;
using UMA.Models;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Models.Entity;
using UMA.Repositories;

namespace UMA.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<object> _passwordHasher = new();

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserRequest> GetUserByEmail(string email)
        {
            return await this._userRepository.GetUserByEmail(email);
        }

        public async Task<bool> AddUserAsync(UserRequest userRequest)
        {
            User user = await this.MapUser(userRequest);
            user.CreatedAt = DateTime.UtcNow;

            return await this._userRepository.AddAsync(user);
        }

        private async Task<User> MapUser(UserRequest userRequest)
        {
            var hashPassword = _passwordHasher.HashPassword(null, userRequest.Password);

            return new User
            {
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                Email = userRequest.Email,
                PasswordHash = hashPassword,
                PathUrl = userRequest.PathUrl,
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await this._userRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateUserAsync(UserRequest userRequest)
        {
            User user = await this.MapUser(userRequest);

            return await this._userRepository.UpdateAsync(user);
        }

        public async Task<UserResponse> ValidateUser(LoginRequest loginRequest)
        {
            UserResponse userResponse = new UserResponse();

            var userExist = await this._userRepository.GetByEmailAsync(loginRequest.Username);

            if (userExist != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(null, userExist.PasswordHash, loginRequest.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    userResponse.Success = true;
                    userResponse.Message = "Login successful";
                }
                else
                {
                    userResponse.Success = false;
                    userResponse.Message = "Wrong Password";
                }
            }
            else
            {
                userResponse.Success = false;
                userResponse.Message = "Email not found";
            }

            return userResponse;
        }
    }
}