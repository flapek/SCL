namespace SCL.Auth.Core
{
    public class JsonWebToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long Expires { get; set; }
        public string Id { get; set; }
    }
}