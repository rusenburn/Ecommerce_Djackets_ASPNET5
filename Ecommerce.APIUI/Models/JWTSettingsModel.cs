using System.Collections.Generic;

namespace Ecommerce.APIUI.Models
{
    public class JWTSettingsModel
    {
        public string Issuer { get; set; }
        public ICollection<string> ValidIssuers { get; set; }
        public string Secret { get; set; }
        public int ExpirationInDays { get; set; }
    }
}