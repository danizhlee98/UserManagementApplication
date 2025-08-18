using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UMA.Data;
using UMA.Models;
using UMA.Models.Entity;

namespace UMA.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserRequest> GetUserByEmail(string email)
        {
            var user = await this._context.Users
                                .AsNoTracking()
                                .Where(col => col.Email.Equals(email))
                                .Select(UserRequest.UserDtoSelector)
                                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<bool> AddAsync(User user)
        {
            await this._context.Users.AddAsync(user);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                var success = await _context.SaveChangesAsync();
                return success > 0;
            }
            return false;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var users = await this._context.Users
                        .Where(col => col.Email.Equals(user.Email))
                        .FirstOrDefaultAsync();

            users.FirstName = user.FirstName;
            users.LastName = user.LastName;
            users.UpdatedAt = DateTime.Now;

            _context.Users.Update(users);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking()
                                       .Where(u => u.Email == email)
                                       .FirstOrDefaultAsync();
        }
    }
}