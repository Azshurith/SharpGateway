using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharpGateway.Models;
using SharpGateway.Services;
using System.ComponentModel.DataAnnotations;

namespace SharpGateway.Controllers
{
    /// <summary>
    /// Handles credit card authorization (without capture).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizeController : AuthorizeNetBase
    {
        private readonly ILogger<AuthorizeController> _logger;

        /// <summary>
        /// Initializes the controller with Authorize.Net credentials and logger.
        /// </summary>
        /// <param name="config">Injected config from appsettings.json.</param>
        /// <param name="logger">Injected logger for structured logging.</param>
        public AuthorizeController(IOptions<AuthorizeNetConfig> config, ILogger<AuthorizeController> logger) : base(config)
        {
            _logger = logger;
        }

        /// <summary>
        /// Authorizes a credit card without capturing the funds.
        /// Use this when you intend to capture the funds later.
        /// </summary>
        /// <param name="request">The full authorization request including customer, billing, and payment details.</param>
        /// <returns>Transaction ID on success; error message on failure.</returns>
        /// <response code="200">Authorization successful</response>
        /// <response code="400">Invalid request or authorization failed</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult Post([FromBody] AuthorizeRequest request)
        {
            _logger.LogInformation("Received authorize request: {@SafeRequest}", new
            {
                request.CustomerId,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Amount,
                request.City,
                request.State,
                request.ShippingCity,
                request.ShippingState,
                request.PONumber,
                request.Tax,
                request.Duty,
                request.Freight,
                request.TaxExempt,
                MaskedCard = MaskCard(request.CardNumber),
                Expiration = MaskExpiration(request.Expiration),
                CVV = "***"
            });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Authorization request model validation failed: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                ValidatePaymentRequest(request);

                var creditCard = new creditCardType
                {
                    cardNumber = request.CardNumber,
                    expirationDate = ParseExpiration(request.Expiration),
                    cardCode = request.CVV
                };

                var billingAddress = new customerAddressType
                {
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    company = request.Company,
                    address = request.Address,
                    city = request.City,
                    state = request.State,
                    zip = request.Zip,
                    country = request.Country,
                    phoneNumber = request.Phone,
                    faxNumber = request.Fax
                };

                var shippingAddress = new nameAndAddressType
                {
                    firstName = request.ShippingFirstName,
                    lastName = request.ShippingLastName,
                    company = request.ShippingCompany,
                    address = request.ShippingAddress,
                    city = request.ShippingCity,
                    state = request.ShippingState,
                    zip = request.ShippingZip,
                    country = request.ShippingCountry
                };

                var customerData = new customerDataType
                {
                    id = request.CustomerId,
                    email = request.Email,
                    type = customerTypeEnum.individual
                };

                var transaction = new createTransactionRequest
                {
                    transactionRequest = new transactionRequestType
                    {
                        transactionType = transactionTypeEnum.authOnlyTransaction.ToString(),
                        amount = request.Amount,
                        payment = new paymentType { Item = creditCard },
                        billTo = billingAddress,
                        shipTo = shippingAddress,
                        customer = customerData,
                        tax = request.Tax.HasValue ? new extendedAmountType { amount = request.Tax.Value, name = "Tax" } : null,
                        duty = request.Duty.HasValue ? new extendedAmountType { amount = request.Duty.Value, name = "Duty" } : null,
                        taxExempt = request.TaxExempt ?? false,
                        poNumber = request.PONumber
                    }
                };

                var controller = new createTransactionController(transaction);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response?.messages.resultCode == messageTypeEnum.Ok && response.transactionResponse?.responseCode == "1")
                {
                    _logger.LogInformation("Authorization successful. Transaction ID: {TransactionId}", response.transactionResponse.transId);

                    return Ok(new
                    {
                        success = true,
                        transactionId = response.transactionResponse.transId
                    });
                }

                var errorMsg = response?.transactionResponse?.errors?[0]?.errorText
                    ?? response?.messages?.message?[0]?.text
                    ?? "Authorization failed.";

                _logger.LogWarning("Authorization declined. Reason: {Error}", errorMsg);

                return BadRequest(new { error = errorMsg });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authorization.");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }
    }
}
