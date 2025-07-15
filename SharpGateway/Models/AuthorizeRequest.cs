using System.ComponentModel.DataAnnotations;

namespace SharpGateway.Models
{
    /// <summary>
    /// Represents a full authorization request including customer, billing, shipping, and transaction details.
    /// </summary>
    public class AuthorizeRequest
    {
        // ==== CUSTOMER BILLING INFORMATION ===

        /// <summary>
        /// Unique customer identifier (used for recurring profiles or tracking).
        /// </summary>
        [StringLength(50)]
        public string? CustomerId { get; set; }

        /// <summary>
        /// First name of the cardholder.
        /// </summary>
        [Required, StringLength(50)]
        public string FirstName { get; set; } = default!;

        /// <summary>
        /// Last name of the cardholder.
        /// </summary>
        [Required, StringLength(50)]
        public string LastName { get; set; } = default!;

        /// <summary>
        /// Billing company name.
        /// </summary>
        [StringLength(100)]
        public string? Company { get; set; }

        /// <summary>
        /// Street address for billing.
        /// </summary>
        [Required, StringLength(100)]
        public string Address { get; set; } = default!;

        /// <summary>
        /// City for billing.
        /// </summary>
        [Required, StringLength(50)]
        public string City { get; set; } = default!;

        /// <summary>
        /// State or province for billing.
        /// </summary>
        [Required, StringLength(20)]
        public string State { get; set; } = default!;

        /// <summary>
        /// ZIP or postal code for billing.
        /// </summary>
        [Required, StringLength(10)]
        public string Zip { get; set; } = default!;

        /// <summary>
        /// Country for billing.
        /// </summary>
        [Required, StringLength(60)]
        public string Country { get; set; } = default!;

        /// <summary>
        /// Customer phone number.
        /// </summary>
        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Customer fax number.
        /// </summary>
        [Phone]
        public string? Fax { get; set; }

        /// <summary>
        /// Email address of the customer.
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }

        // ==== CUSTOMER SHIPPING INFORMATION ===

        /// <summary>
        /// First name for the shipping recipient.
        /// </summary>
        [StringLength(50)]
        public string? ShippingFirstName { get; set; }

        /// <summary>
        /// Last name for the shipping recipient.
        /// </summary>
        [StringLength(50)]
        public string? ShippingLastName { get; set; }

        /// <summary>
        /// Shipping company name.
        /// </summary>
        [StringLength(100)]
        public string? ShippingCompany { get; set; }

        /// <summary>
        /// Shipping street address.
        /// </summary>
        [StringLength(100)]
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Shipping city.
        /// </summary>
        [StringLength(50)]
        public string? ShippingCity { get; set; }

        /// <summary>
        /// Shipping state or province.
        /// </summary>
        [StringLength(20)]
        public string? ShippingState { get; set; }

        /// <summary>
        /// Shipping ZIP or postal code.
        /// </summary>
        [StringLength(10)]
        public string? ShippingZip { get; set; }

        /// <summary>
        /// Shipping country.
        /// </summary>
        [StringLength(60)]
        public string? ShippingCountry { get; set; }

        // ======= ADDITIONAL INFORMATION ======

        /// <summary>
        /// Tax amount applied to the transaction.
        /// </summary>
        public decimal? Tax { get; set; }

        /// <summary>
        /// Duty fee applied to the transaction.
        /// </summary>
        public decimal? Duty { get; set; }

        /// <summary>
        /// Freight or shipping fee.
        /// </summary>
        public decimal? Freight { get; set; }

        /// <summary>
        /// Indicates if the transaction is tax-exempt.
        /// </summary>
        public bool? TaxExempt { get; set; }

        /// <summary>
        /// Optional purchase order number.
        /// </summary>
        [StringLength(25)]
        public string? PONumber { get; set; }

        // ==== PAYMENT ====

        /// <summary>
        /// The full credit card number (13–19 digits).
        /// </summary>
        [Required, RegularExpression(@"^\d{13,19}$", ErrorMessage = "Card number must be 13 to 19 digits.")]
        public string CardNumber { get; set; } = default!;

        /// <summary>
        /// The expiration date of the card in MM/YYYY format.
        /// </summary>
        [Required, RegularExpression(@"^(0[1-9]|1[0-2])\/\d{4}$", ErrorMessage = "Expiration must be in MM/YYYY format.")]
        public string Expiration { get; set; } = default!;

        /// <summary>
        /// The CVV security code (3–4 digits).
        /// </summary>
        [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string CVV { get; set; } = default!;

        /// <summary>
        /// Total amount to authorize for this transaction.
        /// </summary>
        [Required, Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
    }
}
