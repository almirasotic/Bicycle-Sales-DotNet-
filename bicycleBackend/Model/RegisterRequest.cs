namespace bicycleBackend.Model
{
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // ✅ Ne PasswordHash, već Password
        public int Role { get; set; } = 0;
    }
}
