namespace PaymentIntegrationAPI.DTOs.Payment;

public class VerifyPaymentRequest
{
    public string TransactionUuid { get; set; } = string.Empty;
}