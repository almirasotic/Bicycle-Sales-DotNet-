using bicycleBackend.Model;
using bicycleBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace bicycleBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        // ✅ REGISTRACIJA KORISNIKA
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required" });

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hashujemo lozinku
                Role = request.Role
            };

            var createdUser = await _authService.Register(user, request.Password);
            if (createdUser == null)
                return BadRequest(new { message = "User already exists" });

            return Ok(new { message = "Registration successful" });
        }



        // ✅ LOGIN KORISNIKA
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            Console.WriteLine($"🔹 Login pokušaj za: {loginRequest.Email}");

            var authenticatedUser = await _authService.Authenticate(loginRequest.Email, loginRequest.Password);
            if (authenticatedUser == null)
            {
                Console.WriteLine("❌ Neispravni podaci!");
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(authenticatedUser);
            Console.WriteLine("✔ Login uspešan! Token generisan.");

            return Ok(new { token, refreshToken = authenticatedUser.RefreshToken });
        }



        // ✅ LISTA KORISNIKA (Privremeno bez autorizacije)
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetAllUsers();
            return Ok(users);
        }

        // ✅ BRISANJE KORISNIKA
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _authService.DeleteUser(id);
            if (!result)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(new { message = "User deleted successfully." });
        }

        // ✅ GENERISANJE JWT TOKENA
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role.ToString()) // ✅ Dodaj role u token
    };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // ✅ REFRESH TOKEN
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var user = await _authService.GetUserByRefreshToken(refreshToken);
            if (user == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            var newJwtToken = GenerateJwtToken(user);
            return Ok(new { token = newJwtToken });
        }
    }
}
