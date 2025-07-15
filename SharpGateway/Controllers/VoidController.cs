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
    /// Handles voiding of unsettled Authorize.Net transactions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VoidController : AuthorizeNetBase
    {
        private readonly ILogger<VoidController> _logger;

        /// <summary>
        /// Initializes the void controller with Authorize.Net configuration and logger.
        /// </summary>
        /// <param name="config">Injected configuration via IOptions pattern.</param>
        /// <param name="logger">Logger for structured logging.</param>
        public VoidController(IOptions<AuthorizeNetConfig> config, ILogger<VoidController> logger) : base(config)
        {
            _logger = logger;
        }

        /// <summary>
        /// Voids an unsettled transaction (e.g. auth-only or pending charge).
        /// </summary>
        /// <param name="request">The transaction ID to void.</param>
        /// <returns>Void transaction ID on success; error message on failure.</returns>
        /// <response code="200">Void successful</response>
        /// <response code="400">Invalid transaction ID or void failed</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult Post([FromBody] VoidRequest request)
        {
            _logger.LogInformation("Received void request: {@Request}", request);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for void request: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var transaction = new createTransactionRequest
                {
                    transactionRequest = new transactionRequestType
                    {
                        transactionType = transactionTypeEnum.voidTransaction.ToString(),
                        refTransId = request.TransactionId
                    }
                };

                var controller = new createTransactionController(transaction);
                controller.Execute();
                var response = controller.GetApiResponse();

                if (response?.messages.resultCode == messageTypeEnum.Ok && response.transactionResponse?.responseCode == "1")
                {
                    _logger.LogInformation("Void successful. Transaction ID: {VoidId}", response.transactionResponse.transId);

                    return Ok(new
                    {
                        success = true,
                        voidId = response.transactionResponse.transId
                    });
                }

                var errorMsg = response?.transactionResponse?.errors?[0]?.errorText ?? "Void failed.";
                _logger.LogWarning("Void declined. Transaction ID: {TransactionId}, Reason: {Error}", request.TransactionId, errorMsg);

                return BadRequest(new { error = errorMsg });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error during void: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing void.");
                return StatusCode(500, new { error = "Internal server error.", details = ex.Message });
            }
        }
    }
}
