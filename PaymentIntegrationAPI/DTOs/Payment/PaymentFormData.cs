namespace PaymentIntegrationAPI.DTOs.Payment;
public class PaymentFormData
{
    public string Amount { get; set; } = string.Empty;
    public string TaxAmount { get; set; } = string.Empty;
    public string TotalAmount { get; set; } = string.Empty;
    public string TransactionUuid { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductServiceCharge { get; set; } = string.Empty;
    public string ProductDeliveryCharge { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailureUrl { get; set; } = string.Empty;
    public string SignedFieldNames { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
}