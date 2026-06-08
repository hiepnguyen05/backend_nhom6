namespace UserReportService.Domain.Entities;

public class OrderReceipt
{
    public Guid OrderId { get; set; } // PK
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
