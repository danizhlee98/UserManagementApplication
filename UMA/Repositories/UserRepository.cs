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

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await this._context.Users
                                .AsNoTracking()
                                .Where(col => col.Email.Equals(email))
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
            _context.Users.Update(user);
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