namespace Naviam.InternetBank.Entities
{
    public class LoginResponse
    {
        public bool IsAuthenticated { get; set; }
        public int ErrorCode { get; set; }
    }
}