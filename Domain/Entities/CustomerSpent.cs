namespace UserReportService.Domain.Entities;

public class CustomerSpent
{
    public Guid CustomerId { get; set; } // PK
    public required string CustomerName { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
}
