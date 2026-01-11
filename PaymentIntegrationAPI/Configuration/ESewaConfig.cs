namespace PaymentIntegrationAPI.Configuration;

public class ESewaConfig
{
    public string Environment { get; set; } = "UAT";
    public ESewaEnvironment UAT { get; set; } = new();
    public ESewaEnvironment Production { get; set; } = new();
    public string CallbackBaseUrl { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailureUrl { get; set; } = string.Empty;
    public int TimeoutMinutes { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;

    public ESewaEnvironment CurrentEnvironment =>
        Environment == "Production" ? Production : UAT;
}