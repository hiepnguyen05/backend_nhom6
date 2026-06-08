namespace UserReportService.Domain.Entities;

public class DailyRevenue
{
    public DateTime Date { get; set; } // PK: represents the day (Date part only)
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
}
