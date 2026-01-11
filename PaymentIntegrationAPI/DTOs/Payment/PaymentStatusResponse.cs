namespace PaymentIntegrationAPI.DTOs.Payment;
public class PaymentStatusResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaymentStatusData? Data { get; set; }
}