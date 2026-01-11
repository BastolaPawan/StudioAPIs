namespace PaymentIntegrationAPI.Services.Implementations;

using Microsoft.EntityFrameworkCore;
using PaymentIntegrationAPI.Data;
using PaymentIntegrationAPI.Models;
using PaymentIntegrationAPI.Services.Interfaces;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentIntegrationDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(
        PaymentIntegrationDbContext context,
        ILogger<PaymentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentTransaction?> GetByIdAsync(long id)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Bill)
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async Task<PaymentTransaction?> GetByTransactionUuidAsync(string transactionUuid)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.Bill)
            .FirstOrDefaultAsync(pt => pt.TransactionUuid == transactionUuid);
    }

    public async Task<List<PaymentTransaction>> GetByBillIdAsync(long billId)
    {
        return await _context.PaymentTransactions
            .Where(pt => pt.BillId == billId)
            .OrderByDescending(pt => pt.InitiatedAt)
            .ToListAsync();
    }

    public async Task<PaymentTransaction> CreateAsync(PaymentTransaction transaction)
    {
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<PaymentTransaction> UpdateAsync(PaymentTransaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        _context.PaymentTransactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> ExistsAsync(string transactionUuid)
    {
        return await _context.PaymentTransactions
            .AnyAsync(pt => pt.TransactionUuid == transactionUuid);
    }
}