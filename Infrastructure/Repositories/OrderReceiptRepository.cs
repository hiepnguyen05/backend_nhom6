using Microsoft.EntityFrameworkCore;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;
using UserReportService.Infrastructure.Data;

namespace UserReportService.Infrastructure.Repositories;

public class OrderReceiptRepository : IOrderReceiptRepository
{
    private readonly UserDbContext _context;

    public OrderReceiptRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<OrderReceipt?> GetByIdAsync(Guid orderId)
    {
        return await _context.OrderReceipts.FindAsync(orderId);
    }

    public async Task AddAsync(OrderReceipt receipt)
    {
        await _context.OrderReceipts.AddAsync(receipt);
    }
}
