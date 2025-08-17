using System;
using UMA.Models.Entity.BasedEntity;

namespace UMA.Models.Entity
{
    public class User : BaseEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; } 

        public string PasswordHash { get; set; } 

        public string? PathUrl { get; set; }

    }
}