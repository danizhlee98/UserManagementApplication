using System.Collections.Generic;
using System.Threading.Tasks;
using UMA.Models;
using UMA.Models.Dto.Request;
using UMA.Models.Dto.Response;
using UMA.Models.Entity;

namespace UMA.Services
{
    public interface IAuthService
    {
        string GenerateRefreshToken();
    }
}