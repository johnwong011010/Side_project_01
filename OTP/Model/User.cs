namespace OTP.Model
{
    public class User
    {

        public string Username { get; set; }
        public string Password { get; set; }
        public bool isDeleted { get; set; }
        public string? Secret { get; set; }
    }
}
