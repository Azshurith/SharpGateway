using System.ComponentModel.DataAnnotations;

namespace SharpGateway.Models
{
    /// <summary>
    /// Represents a request to void a previously authorized or unsettled transaction.
    /// </summary>
    public class VoidRequest
    {
        /// <summary>
        /// The transaction ID of the payment you want to void.
        /// This must be an unsettled (i.e. not yet captured or refunded) transaction.
        /// </summary>
        [Required(ErrorMessage = "Transaction ID is required.")]
        public string TransactionId { get; set; } = default!;
    }
}
