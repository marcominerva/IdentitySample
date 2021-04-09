namespace IdentitySample.BusinessLayer.Settings
{
    public class JwtSettings
    {
        public string SecurityKey { get; init; }

        public string Issuer { get; init; }

        public string Audience { get; set; }
    }
}
