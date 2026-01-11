namespace PaymentIntegrationAPI.DTOs.Payment;
using System.Text.Json.Serialization;

public class ESewaStatusResponse
{
    [JsonPropertyName("product_code")]
    public string? ProductCode { get; set; }

    [JsonPropertyName("transaction_uuid")]
    public string? TransactionUuid { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("ref_id")]
    public string? RefId { get; set; }
}