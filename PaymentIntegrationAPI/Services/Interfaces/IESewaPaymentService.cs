namespace PaymentIntegrationAPI.Services.Interfaces;

using PaymentIntegrationAPI.DTOs.Payment;

public interface IESewaPaymentService
{
    Task<InitiatePaymentResponse> InitiatePaymentAsync(InitiatePaymentRequest request, string userId);
    Task<bool> VerifyPaymentAsync(string transactionUuid);
    Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionUuid);
    Task<bool> HandleSuccessCallbackAsync(string oid, string amt, string refId);
    Task<bool> HandleFailureCallbackAsync(string oid, string? reason);
    string GenerateSignature(string message, string secretKey);
}