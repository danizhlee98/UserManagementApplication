using System;
using System.Linq.Expressions;
using UMA.Models.Entity;

namespace UMA.Models
{
    public class UserRequest
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; } 

        public string Password { get; set; } 

        public string? PathUrl { get; set; }

        public static Expression<Func<User, UserRequest>> UserDtoSelector =>
       user => new UserRequest
       {
           Email = user.Email,
           FirstName = user.FirstName,
           LastName = user.LastName,
           Password = user.PasswordHash,
           PathUrl = user.PathUrl
       };
    }
}