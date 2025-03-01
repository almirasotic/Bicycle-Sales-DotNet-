using System.ComponentModel.DataAnnotations;

namespace bicycleBackend.Model
{
    public class User
    {
        [Key] // ✅ Primarni ključ
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? RefreshToken { get; set; }
        [Required]
        public int Role { get; set; } = 0;
    }
}
