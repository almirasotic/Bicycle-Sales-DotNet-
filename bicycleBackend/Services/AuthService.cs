using bicycleBackend.Model;
using bicycleBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace bicycleBackend.Services
{
    public class AuthService
    {
        private readonly DataContext _context;

        public AuthService(DataContext context) 
        {
            _context = context;
        }

        public async Task<User?> Register(User user, string password)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return null; // ✅ Ako već postoji korisnik sa tim emailom
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // ✅ Proveri da li korisnik ima postavljenu rolu
            if (user.Role == 0)
                user.Role = 0; // 0 = Običan korisnik, 1 = Admin

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        public async Task<User?> Authenticate(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            user.RefreshToken = GenerateRefreshToken(); // ✅ Generišemo novi RefreshToken
            await _context.SaveChangesAsync(); // ✅ Čuvamo token u bazi

            return user;
        }


        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<bool> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false; 
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true; 
        }
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        public async Task<User?> GetUserByRefreshToken(string refreshToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

    }
}
