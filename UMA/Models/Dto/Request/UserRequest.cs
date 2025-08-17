using System;

namespace UMA.Models
{
    public class UserRequest
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; } 

        public string Password { get; set; } 

        public string? PathUrl { get; set; }
    }
}