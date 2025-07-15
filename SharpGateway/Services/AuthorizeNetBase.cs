using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharpGateway.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SharpGateway.Services
{
    /// <summary>
    /// Abstract base class for Authorize.Net controller functionality.
    /// Sets up merchant authentication and provides utility methods for validation.
    /// Inherits from <see cref="ControllerBase"/> to enable standard ASP.NET Core responses.
    /// </summary>
    public abstract class AuthorizeNetBase : ControllerBase
    {
        /// <summary>
        /// Injected configuration containing Authorize.Net API credentials.
        /// </summary>
        protected readonly AuthorizeNetConfig _config;

        /// <summary>
        /// Initializes the Authorize.Net API environment and sets up merchant authentication.
        /// </summary>
        /// <param name="config">The injected Authorize.Net configuration via IOptions pattern.</param>
        protected AuthorizeNetBase(IOptions<AuthorizeNetConfig> config)
        {
            _config = config.Value;

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
            {
                name = _config.ApiLoginId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _config.TransactionKey
            };
        }

        /// <summary>
        /// Converts expiration date from MM/YYYY to YYYY-MM format.
        /// Throws a <see cref="ValidationException"/> if format is invalid.
        /// </summary>
        /// <param name="exp">The expiration string in MM/YYYY format.</param>
        /// <returns>A formatted expiration string in YYYY-MM format.</returns>
        /// <exception cref="ValidationException">Thrown when the format is incorrect.</exception>
        protected string ParseExpiration(string exp)
        {
            var parts = exp.Split('/');
            if (parts.Length != 2 || parts[0].Length != 2 || parts[1].Length != 4)
                throw new ValidationException("Expiration must be in MM/YYYY format.");

            return $"{parts[1]}-{parts[0]}";
        }

        /// <summary>
        /// Validates a <see cref="AuthorizeRequest"/> object to ensure all required fields are properly formatted.
        /// Throws a <see cref="ValidationException"/> for the first invalid condition found.
        /// </summary>
        /// <param name="req">The payment request to validate.</param>
        /// <exception cref="ValidationException">Thrown when any validation rule is violated.</exception>
        protected void ValidatePaymentRequest(AuthorizeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.CardNumber) || !Regex.IsMatch(req.CardNumber, @"^\d{13,19}$"))
                throw new ValidationException("Card number must be 13 to 19 digits.");

            if (string.IsNullOrWhiteSpace(req.Expiration))
                throw new ValidationException("Expiration is required.");

            ParseExpiration(req.Expiration); // Will throw if invalid

            if (string.IsNullOrWhiteSpace(req.CVV) || !Regex.IsMatch(req.CVV, @"^\d{3,4}$"))
                throw new ValidationException("CVV must be 3 or 4 digits.");

            if (req.Amount <= 0)
                throw new ValidationException("Amount must be greater than zero.");
        }

        /// <summary>
        /// Masks a credit card number by replacing all but the last four digits with 'X'.
        /// </summary>
        /// <param name="cardNumber">The full credit card number as a string.</param>
        /// <returns>The masked card number (e.g., XXXXXXXXXXXX1234), or "****" if input is null or too short.</returns>
        protected string MaskCard(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
                return "****";

            return new string('X', cardNumber.Length - 4) + cardNumber[^4..];
        }

        /// <summary>
        /// Masks the expiration date by replacing the year with asterisks.
        /// </summary>
        /// <param name="expiration">The expiration string in MM/YYYY format.</param>
        /// <returns>The masked expiration string (e.g., 12/****), or "**/****" if input is null or invalid.</returns>
        protected string MaskExpiration(string? expiration)
        {
            if (string.IsNullOrWhiteSpace(expiration) || !expiration.Contains("/"))
                return "**/****";

            return expiration[..3] + "****";
        }
    }
}
