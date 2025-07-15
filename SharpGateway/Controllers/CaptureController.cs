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
    /// Handles the capture of previously authorized transactions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CaptureController : AuthorizeNetBase
    {
        private readonly ILogger<CaptureController> _logger;

        /// <summary>
        /// Initializes the capture controller with Authorize.Net configuration and logger.
        /// </summary>
        /// <param name="config">Injected config (from appsettings.json)</param>
        /// <param name="logger">Logger instance for structured logging</param>
        public CaptureController(IOptions<AuthorizeNetConfig> config, ILogger<CaptureController> logger) : base(config)
        {
            _logger = logger;
        }

        /// <summary>
        /// Captures funds from a previously authorized transaction.
        /// If no amount is specified, it attempts to capture the full authorized amount.
        /// </summary>
        /// <param name="request">The request containing the transaction ID and optional amount.</param>
        /// <returns>Transaction ID on success; error message otherwise.</returns>
        /// <response code="200">Capture successful</response>
        /// <response code="400">Invalid request or capture failed</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult Post([FromBody] CaptureRequest request)
        {
            _logger.LogInformation("Received capture request: {@Request}", request);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid capture model: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),
                    refTransId = request.TransactionId
                };

                if (request.Amount.HasValue)
                {
                    transactionRequest.amount = request.Amount.Value;
                }

                var transaction = new createTransactionRequest
                {
                    transactionRequest = transactionRequest
                };

                var controller = new createTransactionController(transaction);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response?.messages.resultCode == messageTypeEnum.Ok && response.transactionResponse?.responseCode == "1")
                {
                    _logger.LogInformation("Capture successful. Transaction ID: {TransactionId}", response.transactionResponse.transId);

                    return Ok(new
                    {
                        success = true,
                        transactionId = response.transactionResponse.transId
                    });
                }

                var errorMsg = response?.transactionResponse?.errors?[0]?.errorText ?? "Capture failed.";
                _logger.LogWarning("Capture declined for Transaction ID {TransactionId}. Reason: {Error}", request.TransactionId, errorMsg);

                return BadRequest(new { error = errorMsg });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during capture: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during capture.");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }
    }
}
