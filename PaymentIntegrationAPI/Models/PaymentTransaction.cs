namespace PaymentIntegrationAPI.Models;

public class PaymentTransaction
{
    public long Id { get; set; }
    public string TransactionUuid { get; set; } = string.Empty;
    public long BillId { get; set; }
    public long CustomerId { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal DeliveryCharge { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "PENDING";
    public string PaymentMethod { get; set; } = "eSewa";
    public string? ESewaRefId { get; set; }
    public string? ESewaStatus { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Bill? Bill { get; set; }
}