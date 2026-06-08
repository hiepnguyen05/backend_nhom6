namespace UserReportService.Domain.Entities;

public class ProductSold
{
    public DateTime Date { get; set; } // PK Part 1 (Date component only)
    public Guid ProductId { get; set; } // PK Part 2
    public required string ProductName { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
