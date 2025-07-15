namespace SharpGateway.Models
{
    /// <summary>
    /// Represents configuration settings for Authorize.Net API integration.
    /// These values are typically loaded from appsettings.json and injected via IOptions.
    /// </summary>
    public class AuthorizeNetConfig
    {
        /// <summary>
        /// Your Authorize.Net API Login ID.
        /// This identifies your account and must be kept secure.
        /// </summary>
        public string ApiLoginId { get; set; } = default!;

        /// <summary>
        /// Your Authorize.Net Transaction Key.
        /// Used as a secret to sign and authenticate API requests.
        /// </summary>
        public string TransactionKey { get; set; } = default!;
    }
}
