using System.ComponentModel.DataAnnotations;

namespace SharpGateway.Models
{
    /// <summary>
    /// Represents a request to capture funds from a previously authorized transaction.
    /// </summary>
    public class CaptureRequest
    {
        /// <summary>
        /// The unique transaction ID returned from the authorization step.
        /// </summary>
        [Required(ErrorMessage = "Transaction ID is required.")]
        public string TransactionId { get; set; } = default!;

        /// <summary>
        /// The amount to capture.
        /// If omitted, the full authorized amount will be captured.
        /// </summary>
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal? Amount { get; set; }
    }
}
