namespace PaymentIntegrationAPI.Models;

public class Bill
{
    public long Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public decimal Amount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string PaymentStatus { get; set; } = "UNPAID";
    public string? PaymentMethod { get; set; }
    public long? PaymentTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? LastPaymentAttemptAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual PaymentTransaction? PaymentTransaction { get; set; }
}