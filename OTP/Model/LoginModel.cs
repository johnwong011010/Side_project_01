using System.ComponentModel.DataAnnotations;
using OTP.Interface;

namespace OTP.Model
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Detail { get; set; }
        public string Token { get; set; }
        public RefreshToken? RefreshToken { get; set; }
    }
}
