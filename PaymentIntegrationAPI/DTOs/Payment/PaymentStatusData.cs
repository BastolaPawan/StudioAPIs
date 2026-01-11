namespace PaymentIntegrationAPI.DTOs.Payment;

public class PaymentStatusData
{
    public string TransactionUuid { get; set; } = string.Empty;
    public long BillId { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ESewaRefId { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}