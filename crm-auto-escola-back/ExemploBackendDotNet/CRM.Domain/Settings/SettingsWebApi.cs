using Microsoft.Extensions.Configuration;
namespace MPS.Exemplo.Domain.Settings
{
    public class SettingsWebApi
    {
        public SettingsWebApi(IConfiguration configuration)
        {
            AuthSettings = new AuthSettings();
            configuration.GetSection("AuthSettings").Bind(AuthSettings);
        }

        public AuthSettings AuthSettings { get; set; }
    }

    public class AuthSettings
    {
        public AuthSettings()
        {
            Key = string.Empty;
            Issuer = string.Empty;
            Audience = string.Empty;
        }

        public AuthSettings(string key, string issuer, string audience)
        {
            Key = key;
            Issuer = issuer;
            Audience = audience;
        }

        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
