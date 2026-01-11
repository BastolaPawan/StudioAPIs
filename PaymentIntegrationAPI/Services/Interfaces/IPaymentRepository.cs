using PaymentIntegrationAPI.Models;
namespace PaymentIntegrationAPI.Services.Interfaces;
public interface IPaymentRepository
{
    Task<PaymentTransaction?> GetByIdAsync(long id);
    Task<PaymentTransaction?> GetByTransactionUuidAsync(string transactionUuid);
    Task<List<PaymentTransaction>> GetByBillIdAsync(long billId);
    Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction);
    Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction);
    Task<bool> ExistsAsync(string transactionUuid);
}