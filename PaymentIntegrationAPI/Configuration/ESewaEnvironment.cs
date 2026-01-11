namespace PaymentIntegrationAPI.Configuration;

public class ESewaEnvironment
{
    public string BaseUrl { get; set; } = string.Empty;
    public string StatusBaseUrl { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}