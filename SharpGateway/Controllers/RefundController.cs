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
    /// Handles refund transactions for previously settled payments.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RefundController : AuthorizeNetBase
    {
        private readonly ILogger<RefundController> _logger;

        /// <summary>
        /// Initializes the refund controller with Authorize.Net configuration.
        /// </summary>
        /// <param name="config">Injected configuration from appsettings.json.</param>
        /// <param name="logger">Logger for tracking refund operations.</param>
        public RefundController(IOptions<AuthorizeNetConfig> config, ILogger<RefundController> logger) : base(config)
        {
            _logger = logger;
        }

        /// <summary>
        /// Issues a refund for a settled transaction.
        /// Requires the original transaction ID and the last 4 digits of the card.
        /// </summary>
        /// <param name="request">The refund request containing transaction ID, last 4 card digits, and amount.</param>
        /// <returns>Refund transaction ID on success; error message on failure.</returns>
        /// <response code="200">Refund successful</response>
        /// <response code="400">Validation failed or transaction rejected</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult Post([FromBody] RefundRequest request)
        {
            _logger.LogInformation("Received refund request: {@Request}", request);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid refund model: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                // Last 4 digits are padded to form a placeholder card number
                var creditCard = new creditCardType
                {
                    cardNumber = request.CardNumberLast4.PadLeft(16, 'X'),
                    expirationDate = "XXXX" // Required dummy value for refunds
                };

                var transaction = new createTransactionRequest
                {
                    transactionRequest = new transactionRequestType
                    {
                        transactionType = transactionTypeEnum.refundTransaction.ToString(),
                        refTransId = request.TransactionId,
                        amount = request.Amount,
                        payment = new paymentType { Item = creditCard }
                    }
                };

                var controller = new createTransactionController(transaction);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response?.messages.resultCode == messageTypeEnum.Ok && response.transactionResponse?.responseCode == "1")
                {
                    _logger.LogInformation("Refund successful. Refund ID: {RefundId}", response.transactionResponse.transId);

                    return Ok(new
                    {
                        success = true,
                        refundId = response.transactionResponse.transId
                    });
                }

                var errorMsg = response?.transactionResponse?.errors?[0]?.errorText ?? "Refund failed.";
                _logger.LogWarning("Refund declined. Reason: {Error}", errorMsg);

                return BadRequest(new { error = errorMsg });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during refund: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during refund.");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }
    }
}
