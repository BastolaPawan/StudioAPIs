namespace PaymentIntegrationAPI.DTOs.Payment;

public class InitiatePaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentFormData? Data { get; set; }
    public string? TransactionUuid { get; set; }
}
