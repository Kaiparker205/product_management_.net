
namespace WebTestApp.Bl
{
    public class StatisticsViewModel
    {
        public List<string> Labels { get; set; }
        public List<decimal> TotalRevenue { get; set; }
        public List<int> TotalQuantity { get; set; }

        // Additional properties for summary metrics
        public decimal TotalRevenueSum => TotalRevenue.Sum();
        public int TotalOrders => TotalQuantity.Sum();
        public decimal AverageOrderValue => TotalOrders != 0 ? TotalRevenueSum / TotalOrders : 0;
        // Properties for date range filtering
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
