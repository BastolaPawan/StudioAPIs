namespace PaymentIntegrationAPI.Services.Implementations;

using Microsoft.Extensions.Options;
using PaymentIntegrationAPI.Configuration;
using PaymentIntegrationAPI.Data;
using PaymentIntegrationAPI.DTOs.Payment;
using PaymentIntegrationAPI.Models;
using PaymentIntegrationAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class ESewaPaymentService : IESewaPaymentService
{
    private readonly PaymentIntegrationDbContext _context;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOptions<ESewaConfig> _esewaConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ESewaPaymentService> _logger;

    public ESewaPaymentService(
        PaymentIntegrationDbContext context,
        IPaymentRepository paymentRepository,
        IOptions<ESewaConfig> esewaConfig,
        IHttpClientFactory httpClientFactory,
        ILogger<ESewaPaymentService> logger)
    {
        _context = context;
        _paymentRepository = paymentRepository;
        _esewaConfig = esewaConfig;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<InitiatePaymentResponse> InitiatePaymentAsync(
        InitiatePaymentRequest request,
        string userId)
    {
        try
        {
            var bill = await _context.Bills.FindAsync(request.BillId);

            if (bill == null)
            {
                return new InitiatePaymentResponse
                {
                    Success = false,
                    Message = "Bill not found"
                };
            }

            if (bill.PaymentStatus == "PAID")
            {
                return new InitiatePaymentResponse
                {
                    Success = false,
                    Message = "Bill is already paid"
                };
            }

            var transactionUuid = Guid.NewGuid().ToString();
            var amount = bill.Amount;
            var taxAmount = bill.TaxAmount ?? 0;
            var serviceCharge = 0m;
            var deliveryCharge = 0m;
            var totalAmount = amount + taxAmount + serviceCharge + deliveryCharge;

            var config = _esewaConfig.Value;
            var env = config.CurrentEnvironment;

            var message =
                $"total_amount={totalAmount},transaction_uuid={transactionUuid},product_code={env.ProductCode}";
            var signature = GenerateSignature(message, env.SecretKey);

            var transaction = new PaymentTransaction
            {
                TransactionUuid = transactionUuid,
                BillId = bill.Id,
                CustomerId = bill.CustomerId,
                Amount = amount,
                TaxAmount = taxAmount,
                ServiceCharge = serviceCharge,
                DeliveryCharge = deliveryCharge,
                TotalAmount = totalAmount,
                Status = "PENDING",
                PaymentMethod = "eSewa",
                InitiatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                RequestData = JsonSerializer.Serialize(request)
            };

            await _paymentRepository.CreateAsync(transaction);

            bill.LastPaymentAttemptAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment initiated - TransactionUuid: {TransactionUuid}, BillId: {BillId}, Amount: {Amount}",
                transactionUuid, bill.Id, totalAmount);

            var successUrl = $"{config.CallbackBaseUrl}{config.SuccessUrl}";
            var failureUrl = $"{config.CallbackBaseUrl}{config.FailureUrl}";

            var paymentData = new PaymentFormData
            {
                Amount = amount.ToString("F2"),
                TaxAmount = taxAmount.ToString("F2"),
                TotalAmount = totalAmount.ToString("F2"),
                TransactionUuid = transactionUuid,
                ProductCode = env.ProductCode,
                ProductServiceCharge = serviceCharge.ToString("F2"),
                ProductDeliveryCharge = deliveryCharge.ToString("F2"),
                SuccessUrl = successUrl,
                FailureUrl = failureUrl,
                SignedFieldNames = "total_amount,transaction_uuid,product_code",
                Signature = signature,
                PaymentUrl = $"{env.BaseUrl}/api/epay/main/v2/form"
            };

            return new InitiatePaymentResponse
            {
                Success = true,
                Message = "Payment initiated successfully",
                Data = paymentData,
                TransactionUuid = transactionUuid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for BillId: {BillId}", request.BillId);
            return new InitiatePaymentResponse
            {
                Success = false,
                Message = "Failed to initiate payment. Please try again."
            };
        }
    }

    public async Task<bool> VerifyPaymentAsync(string transactionUuid)
    {
        try
        {
            var transaction = await _paymentRepository.GetByTransactionUuidAsync(transactionUuid);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found: {TransactionUuid}", transactionUuid);
                return false;
            }

            var config = _esewaConfig.Value;
            var env = config.CurrentEnvironment;

            var url = $"{env.StatusBaseUrl}/api/epay/transaction/status/" +
                      $"?product_code={env.ProductCode}" +
                      $"&total_amount={transaction.TotalAmount}" +
                      $"&transaction_uuid={transactionUuid}";

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("eSewa verification response: {Content}", content);

            if (response.IsSuccessStatusCode)
            {
                var statusResponse = JsonSerializer.Deserialize<ESewaStatusResponse>(content);

                if (statusResponse?.Status == "COMPLETE")
                {
                    transaction.Status = "COMPLETE";
                    transaction.ESewaStatus = statusResponse.Status;
                    transaction.ESewaRefId = statusResponse.RefId;
                    transaction.CompletedAt = DateTime.UtcNow;
                    transaction.VerifiedAt = DateTime.UtcNow;
                    transaction.ResponseData = content;

                    await _paymentRepository.UpdateAsync(transaction);

                    var bill = await _context.Bills.FindAsync(transaction.BillId);
                    if (bill != null)
                    {
                        bill.PaymentStatus = "PAID";
                        bill.PaymentMethod = "eSewa";
                        bill.PaymentTransactionId = transaction.Id;
                        bill.PaidAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation(
                        "Payment verified - TransactionUuid: {TransactionUuid}, RefId: {RefId}",
                        transactionUuid, statusResponse.RefId);

                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment: {TransactionUuid}", transactionUuid);
            return false;
        }
    }

    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionUuid)
    {
        try
        {
            var transaction = await _paymentRepository.GetByTransactionUuidAsync(transactionUuid);

            if (transaction == null)
            {
                return new PaymentStatusResponse
                {
                    Success = false,
                    Message = "Transaction not found"
                };
            }

            return new PaymentStatusResponse
            {
                Success = true,
                Message = "Payment status retrieved successfully",
                Data = new PaymentStatusData
                {
                    TransactionUuid = transaction.TransactionUuid,
                    BillId = transaction.BillId,
                    BillNumber = transaction.Bill?.BillNumber ?? "",
                    TotalAmount = transaction.TotalAmount,
                    Status = transaction.Status,
                    ESewaRefId = transaction.ESewaRefId,
                    InitiatedAt = transaction.InitiatedAt,
                    CompletedAt = transaction.CompletedAt,
                    FailureReason = transaction.FailureReason
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status: {TransactionUuid}", transactionUuid);
            return new PaymentStatusResponse
            {
                Success = false,
                Message = "Failed to retrieve payment status"
            };
        }
    }

    public async Task<bool> HandleSuccessCallbackAsync(string oid, string amt, string refId)
    {
        try
        {
            _logger.LogInformation("Success callback - OID: {OID}, Amount: {Amount}, RefId: {RefId}", oid, amt, refId);

            var isVerified = await VerifyPaymentAsync(oid);
            return isVerified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling success callback: {OID}", oid);
            return false;
        }
    }

    public async Task<bool> HandleFailureCallbackAsync(string oid, string? reason)
    {
        try
        {
            _logger.LogWarning("Failure callback - OID: {OID}, Reason: {Reason}", oid, reason);

            var transaction = await _paymentRepository.GetByTransactionUuidAsync(oid);
            if (transaction != null)
            {
                transaction.Status = "FAILED";
                transaction.FailureReason = reason ?? "Payment cancelled or failed";
                await _paymentRepository.UpdateAsync(transaction);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling failure callback: {OID}", oid);
            return false;
        }
    }

    public string GenerateSignature(string message, string secretKey)
    {
        var encoding = new UTF8Encoding();
        byte[] keyByte = encoding.GetBytes(secretKey);
        byte[] messageBytes = encoding.GetBytes(message);

        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }
}