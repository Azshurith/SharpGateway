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
    /// Handles immediate payment capture transactions (authorization + capture in one step).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargeController : AuthorizeNetBase
    {
        private readonly ILogger<ChargeController> _logger;

        /// <summary>
        /// Initializes the charge controller with Authorize.Net configuration.
        /// </summary>
        /// <param name="config">Injected config via IOptions from appsettings.json.</param>
        /// <param name="logger">Logger for structured log output.</param>
        public ChargeController(IOptions<AuthorizeNetConfig> config, ILogger<ChargeController> logger)  : base(config)
        {
            _logger = logger;
        }

        /// <summary>
        /// Charges a credit card by authorizing and capturing the specified amount.
        /// </summary>
        /// <param name="request">The payment request including card number, expiration, CVV, and amount.</param>
        /// <returns>Transaction ID on success; error message otherwise.</returns>
        /// <response code="200">Charge successful</response>
        /// <response code="400">Invalid request or card rejected</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult Post([FromBody] AuthorizeRequest request)
        {
            _logger.LogInformation("Received charge request: {@Request}", request);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid charge model: {@ModelState}", ModelState);
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

                var transaction = new createTransactionRequest
                {
                    transactionRequest = new transactionRequestType
                    {
                        transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                        amount = request.Amount,
                        payment = new paymentType { Item = creditCard }
                    }
                };

                var controller = new createTransactionController(transaction);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response?.messages.resultCode == messageTypeEnum.Ok && response.transactionResponse?.responseCode == "1")
                {
                    _logger.LogInformation("Charge successful. Transaction ID: {TransactionId}", response.transactionResponse.transId);

                    return Ok(new
                    {
                        success = true,
                        transactionId = response.transactionResponse.transId
                    });
                }

                var errorMsg = response?.transactionResponse?.errors?[0]?.errorText ?? "Charge failed.";
                _logger.LogWarning("Charge declined. Reason: {Error}", errorMsg);

                return BadRequest(new { error = errorMsg });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during charge: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during charge.");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }
    }
}
