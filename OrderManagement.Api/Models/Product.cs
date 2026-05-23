namespace OrderManagement.Api.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public OrderStatus Status { get; set; } = OrderStatus.Iniciado;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid BuyerId { get; set; }
        public Buyer Buyer { get; set; } = null!;

        public List<Product> Products { get; set; } = new();
    }
}
