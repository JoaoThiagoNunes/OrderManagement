using OrderManagement.Api.Models;

namespace OrderManagement.Api.App.DTOs
{
    public class OrderFilterDto
    {
        public OrderStatus? Status { get; set; }
        public string? BuyerName { get; set; }
    }
}
