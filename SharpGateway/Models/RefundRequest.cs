using System.ComponentModel.DataAnnotations;

namespace SharpGateway.Models
{
    /// <summary>
    /// Represents a request to refund a previously settled transaction.
    /// </summary>
    public class RefundRequest
    {
        /// <summary>
        /// The original transaction ID that you want to refund.
        /// </summary>
        [Required(ErrorMessage = "Transaction ID is required.")]
        public string TransactionId { get; set; } = default!;

        /// <summary>
        /// The last 4 digits of the card used in the original transaction.
        /// Required by Authorize.Net for refund requests.
        /// </summary>
        [Required(ErrorMessage = "Last 4 digits of the card number are required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Card number must be exactly 4 digits.")]
        public string CardNumberLast4 { get; set; } = default!;

        /// <summary>
        /// The amount to refund. Must not exceed the original charged amount.
        /// </summary>
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
