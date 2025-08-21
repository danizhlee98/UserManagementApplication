using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using UMA.Models;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Models.Entity;
using UMA.Repositories;

namespace UMA.Services
{
    public class AuthService : IAuthService
    {

        public AuthService()
        {
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}